using ABSA.RD.S3Proxy.Proxy;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;

namespace ABSA.RD.S3Proxy.Misc
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddS3Proxy(this IServiceCollection service, S3Config config)
        {
            var s3 = CreateS3Client(config);
            service.AddSingleton(typeof(IProxy), new Proxy.S3Proxy(s3));

            return service;
        }

        private static IAmazonS3 CreateS3Client(S3Config config)
        {
            var s3cfg = new AmazonS3Config()
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(config.Region)
            };

            if (config.NoProxy)
                s3cfg.SetWebProxy(new WebProxy());

            if (config.TimeoutSeconds != 0)
                s3cfg.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);

            AWSCredentials credentials;
            if (string.IsNullOrEmpty(config.CredentialsProfile))
                credentials = FallbackCredentialsFactory.GetCredentials();
            else if (!new CredentialProfileStoreChain().TryGetAWSCredentials(config.CredentialsProfile, out credentials))
                throw new Exception($"Cannot obtain credentials for profile {config.CredentialsProfile}");

            return new AmazonS3Client(credentials, s3cfg);
        }
    }
}
