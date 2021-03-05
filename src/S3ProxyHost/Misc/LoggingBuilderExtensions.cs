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
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Time;
using System;
using System.IO;

namespace ABSA.RD.S4.S3ProxyHost.Misc
{
    public static class LoggingBuilderExtensions
    {
        public static ILoggingBuilder InitializeNLog(this ILoggingBuilder builder)
        {
            TimeSource.Current = new AccurateLocalTimeSource();

            // Nlog.Config is using following variables we have to define.
            GlobalDiagnosticsContext.Set("LogDir", Path.Combine(Path.GetTempPath(), "ABSA_OSS_S4_logs"));
            GlobalDiagnosticsContext.Set("StartTime", DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss"));

            // delete old logs
            if (LogManager.Configuration.Variables.TryGetValue("deleteFilesAfterDays", out var deleteInterval))
            {
                if (Directory.Exists(GlobalDiagnosticsContext.Get("LogDir")))
                {
                    var files = Directory.EnumerateFiles(GlobalDiagnosticsContext.Get("LogDir"), "*." + LogManager.Configuration.Variables["logFileSuffix"].Text);
                    foreach (var f in files)
                    {
                        if (new FileInfo(f).CreationTimeUtc + TimeSpan.FromDays(int.Parse(deleteInterval.Text)) < DateTime.UtcNow)
                            File.Delete(f);
                    }
                }
            }

            return builder;
        }
    }
}