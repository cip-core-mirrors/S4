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

            var watch = Stopwatch.StartNew();
            await RunPutsAsync();
            for (var i = 0; i < _settings.Iterations; ++i)
                await RunGetsAsync();
            await RunDeletesAsync();
            watch.Stop();

            var memUsage = Process.GetCurrentProcess().PeakWorkingSet64;
            var unit = GetUnit(memUsage, out var mulKoef);
            Console.WriteLine($"FINISHED in {watch.Elapsed} with {memUsage / mulKoef} {unit} Peak Workset");
        }

        private async Task RunPutsAsync()
        {
            var elapsed = RunParallelTasksAsync(threadIndex => PutLoopAsync(threadIndex));
            PrintTotals("PUT", await elapsed);
            PrintStats( _handler.TimePut.ToArray());
        }

        private async Task RunGetsAsync()
        {
            _handler.TimeGetFirstByte.Clear();
            _handler.TimeGetLastByte.Clear();

            var elapsed = RunParallelTasksAsync(threadIndex => GetLoopAsync(threadIndex));
            PrintTotals("GET", await elapsed);
            PrintStats(_handler.TimeGetLastByte.ToArray(), _handler.TimeGetFirstByte.ToArray());
        }

        private async Task RunDeletesAsync()
        {
            var elapsed = RunParallelTasksAsync(threadIndex => DeleteLoopAsync(threadIndex));
            PrintTotals("DELETE", await elapsed);
            PrintStats(_handler.TimeDelete.ToArray());
        }

        private async Task<TimeSpan> RunParallelTasksAsync(Func<int, Task> action)
        {
            var watch = Stopwatch.StartNew();
            var tasks = new Task[_settings.ParallelTasks];
            for (var i = 0; i < tasks.Length; ++i)
                tasks[i] = action(i);

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
            var totalSize = (long)totalItems * _settings.ItemSize;
            var speedInBytes = (long)(totalSize / elapsed.TotalSeconds);
            var unit = GetUnit(speedInBytes, out var mulKoef);

            Console.WriteLine("====================");
            Console.WriteLine($"{operation,7} | {elapsed} | {(int)(totalItems / elapsed.TotalSeconds),7} /s | {speedInBytes / mulKoef,7} {unit}/s");
        }

        private void PrintStats(TimeSpan[] timings, TimeSpan[] delay = null)
        {
            const int align = 6;
            const int titleAlign = 10;

            string lbl(string l) => $"{l,align}";
            Console.WriteLine();
            Console.WriteLine($"{"",titleAlign}{lbl("AVG")}{lbl("MIN")}{lbl("P10")}{lbl("P25")}{lbl("P50")}{lbl("P75")}{lbl("P90")}{lbl("P95")}{lbl("P99")}{lbl("MAX")}");

            string time(TimeSpan t) => $"{(int)t.TotalMilliseconds,align}";
            if (delay != null)
            {
                Array.Sort(delay);
                PrintStats_Row($"{"delay ms",titleAlign}", delay, time);
            }

            Array.Sort(timings);
            PrintStats_Row($"{"req ms",titleAlign}", timings, time);

            var min = timings[0];
            var unit = GetUnit((int)(_settings.ItemSize / min.TotalSeconds), out var mulKoef);
            string speed(TimeSpan t) => $"{(int)(_settings.ItemSize / t.TotalSeconds / mulKoef),align}";
            PrintStats_Row($"{$" {unit}/s",titleAlign}", timings, speed);
        }

       private void PrintStats_Row(string title, TimeSpan[] timings, Func<TimeSpan, string> prn)
        {
            var min = timings[0];
            var avg = TimeSpan.FromSeconds(timings.Average(x => x.TotalSeconds));
            var max = timings[^1];
            TimeSpan perc(int p) => timings[timings.Length * p / 100];
            Console.WriteLine($"{title}{prn(avg)}{prn(min)}{prn(perc(10))}{prn(perc(25))}{prn(perc(50))}{prn(perc(75))}{prn(perc(90))}{prn(perc(95))}{prn(perc(99))}{prn(max)}");
        }

        private string GetUnit(long value, out int koef)
        {
            var index = 0;
            koef = 1;
            while (value > 1024 && index < Mul.Length)
            {
                koef *= 1024;
                value /= 1024;
                ++index;
            }

            return Mul[index];
        }
    }
}