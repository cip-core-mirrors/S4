using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Amazon.Runtime.CredentialManagement;
using ABSA.RD.S4.S3Proxy.Misc;
using ABSA.RD.S4.S3Proxy;
using Microsoft.Extensions.Configuration;
using NLog;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using ABSA.RD.S4.S3ProxyHost.Misc;
using Microsoft.AspNetCore.Diagnostics;
using System.Text;
using System;

namespace ABSA.RD.S4.S3ProxyHost
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IWebHostEnvironment env)
        {
            LogManager.LoadConfiguration($"NLog.{env.EnvironmentName}.config");

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {   
            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddNLog();
                logging.InitializeNLog();
            });

            services.AddCors();

            var config = new S3Config
            {
                Region = Configuration["S3:Region"],
                KmsKey = Configuration["S3:KmsKey"],
                CredentialsProfile = new CredentialProfileStoreChain().TryGetAWSCredentials(Configuration["S3:CredentialsProfile"], out var _) ? Configuration["S3:CredentialsProfile"] : null
            };

            services.AddS3Proxy(config);
            services.AddSingleton<ProxyWorker>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Run(async context =>
            {
                var worker = context.RequestServices.GetRequiredService<ProxyWorker>();
                try
                {
                    await worker.Run(context);
                }
                catch(Exception ex)
                {
                    context.Response.StatusCode = 500;
                    await context.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(ex.Message));
                    throw;
                }
            });
        }
    }
}