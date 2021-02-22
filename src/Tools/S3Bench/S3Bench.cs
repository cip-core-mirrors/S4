using ABSA.RD.S4.S3Bench.Settings;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ABSA.RD.S4.S3Bench
{
    class S3Bench
    {
        private static readonly string[] Mul = { "B", "kB", "MB", "GB", "TB", "PB", "EB", "YB", "ZB", "OF" };

        private readonly S3Handler _handler;
        private readonly BenchSettings _settings;

        public S3Bench(S3Handler handler, BenchSettings settings)
        {
            _handler = handler;
            _settings = settings;
        }

        public async Task RunTestAsync()
        {
            Console.WriteLine($"ParallelTasks: {_settings.ParallelTasks}");
            Console.WriteLine($"Items/Task:    {_settings.ItemsPerTask}");
            Console.WriteLine($"Item Size:     {_settings.ItemSize}");

            await RunPutsAsync();
            for (var i = 0; i < _settings.Iterations; ++i)
                await RunGetsAsync();
            await RunDeletesAsync();
        }

        private async Task RunPutsAsync()
        {
            var elapsed = RunParallelTasksAsync(threadIndex => PutLoopAsync(threadIndex));
            PrintTotals("PUT", await elapsed);
            PrintStats("Request", _handler.TimePut.ToArray(), true);
        }

        private async Task RunGetsAsync()
        {
            _handler.TimeGetFirstByte.Clear();
            _handler.TimeGetLastByte.Clear();

            var elapsed = RunParallelTasksAsync(threadIndex => GetLoopAsync(threadIndex));

            PrintTotals("GET", await elapsed);
            PrintStats("Delay", _handler.TimeGetFirstByte.ToArray(), false);
            PrintStats("Request", _handler.TimeGetLastByte.ToArray(), true);
        }

        private async Task RunDeletesAsync()
        {
            var elapsed = RunParallelTasksAsync(threadIndex => DeleteLoopAsync(threadIndex));

            PrintTotals("DELETE", await elapsed);
            PrintStats("Request", _handler.TimeDelete.ToArray(), true);
        }

        private async Task<TimeSpan> RunParallelTasksAsync(Func<int, Task> action)
        {
            var watch = Stopwatch.StartNew();
            var tasks = new Task[_settings.ParallelTasks];
            for (var i = 0; i < tasks.Length; ++i)
            {
                var local = i;
                tasks[i] = action(local);
            }

            await Task.WhenAll(tasks);

            return watch.Elapsed;
        }

        private async Task PutLoopAsync(int threadIndex)
        {
            var item = new byte[_settings.ItemSize];

            for (var i = 0; i < _settings.ItemsPerTask; ++i)
            {
                for (var j = 0; j < item.Length; ++j)
                    item[j] = (byte)(i + j + threadIndex);

                await _handler.PutAsync(item, $"T{threadIndex}I{i}");
            }
        }

        private async Task GetLoopAsync(int threadIndex)
        {
            for (var i = 0; i < _settings.ItemsPerTask; ++i)
                await _handler.GetAsync($"T{threadIndex}I{i}");
        }

        private async Task DeleteLoopAsync(int threadIndex)
        {
            for (var i = 0; i < _settings.ItemsPerTask; ++i)
                await _handler.DeleteAsync($"T{threadIndex}I{i}");
        }

        private void PrintTotals(string operation, TimeSpan elapsed)
        {
            var totalItems = _settings.ParallelTasks * _settings.ItemsPerTask;
            var totalSize = totalItems * _settings.ItemSize;
            var speedInBytes = (int)(totalSize / elapsed.TotalSeconds);
            var unit = GetUnit(speedInBytes, out var mulKoef);

            Console.WriteLine("====================");
            Console.WriteLine($"{operation,7} | {elapsed} | {(int)(totalItems / elapsed.TotalSeconds),7} /s | {speedInBytes / mulKoef,7} {unit}/s");
        }

        private void PrintStats(string title, TimeSpan[] timings, bool withbps)
        {
            Array.Sort(timings);

            const int align = 7;
            const int titleAlign = 7;
            var mulKoef = 1;
            var min = timings[0];
            var avg = TimeSpan.FromSeconds(timings.Average(x => x.TotalSeconds));
            var max = timings[^1];
            
            Func<int, TimeSpan> perc = p => timings[timings.Length * p / 100];
            Func<string, string> lbl = l => $"{l,align}";
            Func<TimeSpan, string> time = t => $"{(int)t.TotalMilliseconds,align}";
            Func<TimeSpan, string> speed = t => $"{(int)(_settings.ItemSize / t.TotalSeconds / mulKoef),align}";

            Console.WriteLine();
            Console.WriteLine($"{title,titleAlign}{lbl("AVG")}{lbl("MIN")}{lbl("P10")}{lbl("P25")}{lbl("P50")}{lbl("P75")}{lbl("P90")}{lbl("P95")}{lbl("P99")}{lbl("MAX")}");
            Console.WriteLine($"{" ms",titleAlign}{time(avg)}{time(min)}{time(perc(10))}{time(perc(25))}{time(perc(50))}{time(perc(75))}{time(perc(90))}{time(perc(95))}{time(perc(99))}{time(max)}");

            if (withbps)
            {
                var unit = GetUnit((int)(_settings.ItemSize / min.TotalSeconds), out mulKoef);
                Console.WriteLine($"{$" {unit}/s",titleAlign}{speed(avg)}{speed(min)}{speed(perc(10))}{speed(perc(25))}{speed(perc(50))}{speed(perc(75))}{speed(perc(90))}{speed(perc(95))}{speed(perc(99))}{speed(max)}");
            }

        }

        private string GetUnit(int value, out int koef)
        {
            var index = 0;
            koef = 1;
            while (value > 1024)
            {
                koef *= 1024;
                value /= 1024;
                ++index;
            }

            return Mul[index];
        }
    }
}