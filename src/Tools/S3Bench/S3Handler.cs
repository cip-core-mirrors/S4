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
using System.Threading.Tasks;

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

        public async Task PutAsync(byte[] data, string id)
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
            await _client.PutObjectAsync(request);
            TimePut.Add(watch.Elapsed);
        }

        public async Task<byte[]> GetAsync(string id)
        {
            var request = new GetObjectRequest
            {
                BucketName = _settings.Bucket,
                Key = _settings.Prefix + id
            };

            var watch = Stopwatch.StartNew();
            var response = await _client.GetObjectAsync(request);
            var result = new byte[response.ContentLength];
            result[0] = (byte)response.ResponseStream.ReadByte();
            TimeGetFirstByte.Add(watch.Elapsed);

            var index = 1;
            while (index < result.Length)
                index += await response.ResponseStream.ReadAsync(result, index, result.Length - index);
            TimeGetLastByte.Add(watch.Elapsed);

            return result;
        }

        public async Task DeleteAsync(string id)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _settings.Bucket,
                Key = _settings.Prefix + id
            };

            var watch = Stopwatch.StartNew();
            await _client.DeleteObjectAsync(request);
            TimeDelete.Add(watch.Elapsed);
        }
    }
}