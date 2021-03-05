//   Copyright 2021 Absa Group
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
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