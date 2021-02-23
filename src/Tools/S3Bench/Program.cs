using ABSA.RD.S4.S3Bench.Settings;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace ABSA.RD.S4.S3Bench
{
    class Program
    {
        static async Task Main()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var s3Handler = new S3Handler(config.GetSection("S3").Get<S3Settings>());
            var bench = new S3Bench(s3Handler, config.GetSection("Bench").Get<BenchSettings>());
            await bench.RunTestAsync();
            Console.ReadKey();
        }
    }
}