using ABSA.RD.S4.S3Bench.Settings;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace ABSA.RD.S4.S3Bench
{
    class S3Handler
    {
        private readonly S3Settings _settings;
        private readonly AmazonS3Client _client;

        public readonly ConcurrentBag<TimeSpan> TimeGetFirstByte = new ConcurrentBag<TimeSpan>();
        public readonly ConcurrentBag<TimeSpan> TimeGetLastByte = new ConcurrentBag<TimeSpan>();
        public readonly ConcurrentBag<TimeSpan> TimePut = new ConcurrentBag<TimeSpan>();
        public readonly ConcurrentBag<TimeSpan> TimeDelete = new ConcurrentBag<TimeSpan>();

        public S3Handler(S3Settings settings)
        {
            _settings = settings;

            var s3cfg = new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(_settings.Region) };

            if (!string.IsNullOrEmpty(_settings.Endpoint))
            {
                s3cfg.ServiceURL = _settings.Endpoint;
                s3cfg.ForcePathStyle = true;
            }

            if (_settings.NoProxy)
                s3cfg.SetWebProxy(new WebProxy());

            if (_settings.TimeoutSeconds != 0)
                s3cfg.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

            AWSCredentials credentials;
            if (string.IsNullOrEmpty(_settings.CredentialsProfile))
                credentials = FallbackCredentialsFactory.GetCredentials();
            else if (!new CredentialProfileStoreChain().TryGetAWSCredentials(_settings.CredentialsProfile, out credentials))
                throw new Exception($"Cannot obtain credentials for profile {_settings.CredentialsProfile}");

            _client = new AmazonS3Client(credentials, s3cfg);
        }

        public void Put(byte[] data, string id)
        {
               var request = new PutObjectRequest
            {
                BucketName = _settings.Bucket,
                Key = _settings.Prefix + id,
                InputStream = new MemoryStream(data),
                ServerSideEncryptionMethod = string.IsNullOrEmpty(_settings.KmsKey) ? ServerSideEncryptionMethod.None : ServerSideEncryptionMethod.AWSKMS,
                ServerSideEncryptionKeyManagementServiceKeyId = _settings.KmsKey
            };

            var watch = Stopwatch.StartNew();
            _client.PutObjectAsync(request).GetAwaiter().GetResult();
            TimePut.Add(watch.Elapsed);
        }

        public byte[] Get(string id)
        {
            var request = new GetObjectRequest
            {
                BucketName = _settings.Bucket,
                Key = _settings.Prefix + id
            };

            var watch = Stopwatch.StartNew();
            var response = _client.GetObjectAsync(request).GetAwaiter().GetResult();
            var result = new byte[response.ContentLength];
            result[0] = (byte)response.ResponseStream.ReadByte();
            TimeGetFirstByte.Add(watch.Elapsed);

            var index = 1;
            while (index < result.Length)
                index += response.ResponseStream.Read(result, index, result.Length - index);
            TimeGetLastByte.Add(watch.Elapsed);

            return result;
        }

        public void Delete(string id)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _settings.Bucket,
                Key = _settings.Prefix + id
            };

            var watch = Stopwatch.StartNew();
            _client.DeleteObjectAsync(request).GetAwaiter().GetResult();
            TimeDelete.Add(watch.Elapsed);
        }
    }
}