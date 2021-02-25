using ABSA.RD.S4.S3Proxy.Proxy;
using ABSA.RD.S4.S3Proxy.S3Client;
using ABSA.RD.S4.S3Proxy.S3Client.Archives;
using ABSA.RD.S4.S3Proxy.S3Client.Archives.Extractors;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;

namespace ABSA.RD.S4.S3Proxy.Misc
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddS3Proxy(this IServiceCollection service, S3Config config)
        {
            service.AddSingleton<IArchiveInfoStorage, ArchiveInfoMemoryStorage>();

            service.AddSingleton(
                typeof(IProxy), 
                r => new Proxy.S3Proxy(
                    CreateS3Client(
                        r.GetRequiredService<IArchiveInfoStorage>(),
                        CreateArchiveExtractorsMapping(),
                        config)));

            return service;
        }

        private static IDictionary<string, IArchiveExtractor> CreateArchiveExtractorsMapping()
        {
            return new Dictionary<string, IArchiveExtractor>
            {
                { ".zip", new ZipExtractor() }
            };
        }

        private static IAmazonS3 CreateS3Client(
            IArchiveInfoStorage archivesInfoStorage,
            IDictionary<string, IArchiveExtractor> extractorsMapping,
            S3Config config)
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

            return new ExtendedS3Client(archivesInfoStorage, extractorsMapping, credentials, s3cfg);
        }
    }
}
