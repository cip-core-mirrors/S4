using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using Amazon.Runtime.CredentialManagement;
using ABSA.RD.S4.S3Proxy.Misc;
using ABSA.RD.S4.S3Proxy;

namespace ABSA.RD.S4.S3ProxyHost
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var config = new S3Config
            {
                Region = "eu-west-1",
                KmsKey = "arn:aws:kms:eu-west-1:***REMOVED***",
                CredentialsProfile = new CredentialProfileStoreChain().TryGetAWSCredentials("saml", out var _) ? "saml" : null
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
                catch (Exception ex)
                {

                }
            });
        }
    }
}