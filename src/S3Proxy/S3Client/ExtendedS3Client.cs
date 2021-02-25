using ABSA.RD.S4.S3Proxy.Misc;
using ABSA.RD.S4.S3Proxy.S3Client.Archives;
using ABSA.RD.S4.S3Proxy.S3Client.Archives.Extractors;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ABSA.RD.S4.S3Proxy.S3Client
{
    class ExtendedS3Client : AmazonS3Client
    {
        private readonly IArchiveInfoStorage _archiveInfoStorage;
        private readonly IDictionary<string, IArchiveExtractor> _extractorsMapping;

        public ExtendedS3Client(
            IArchiveInfoStorage archiveInfoStorage,
            IDictionary<string, IArchiveExtractor> extractorsMapping,
            AWSCredentials credentials,
            AmazonS3Config config)
            : base(credentials, config)
        {
            _archiveInfoStorage = archiveInfoStorage;
            _extractorsMapping = extractorsMapping;
        }

        public override async Task<ListObjectsV2Response> ListObjectsV2Async(ListObjectsV2Request request, CancellationToken cancellationToken = default)
        {
            var extractor = _extractorsMapping.SingleOrDefault(x => request.Prefix.EndsWith(x.Key, StringComparison.OrdinalIgnoreCase)).Value;

            if (extractor == default) // normal case, no need to use any archive extractors
                return await base.ListObjectsV2Async(request, cancellationToken);
            else
            {
                var archiveUniqueReference = CreateArchiveReference(request.BucketName, request.Prefix);
                if (!_archiveInfoStorage.TryGet(archiveUniqueReference, out var info))
                {
                    // download archive from S3
                    var @object = await GetObjectAsync(request.BucketName, request.Prefix, cancellationToken);
                    var data = await @object.ReadData();

                    // extract entries info and save it
                    info = extractor.ExtractInfo(data);
                    _archiveInfoStorage.Save(archiveUniqueReference, info);
                }

                // create response using found info
                var response = new ListObjectsV2Response
                {
                    HttpStatusCode = HttpStatusCode.OK,
                    Name = request.BucketName,
                    Prefix = request.Prefix,
                    KeyCount = Math.Max(info.Entries.Length, request.MaxKeys),
                    Delimiter = request.Delimiter,
                    Encoding = request.Encoding,
                    MaxKeys = request.MaxKeys,
                    StartAfter = request.StartAfter,
                    ContinuationToken = request.ContinuationToken,
                    S3Objects = info.Entries.Select(e => new S3Object
                    {
                        BucketName = request.BucketName,
                        ETag = $"\"{e.LastModified.Ticks:x}\"",
                        Key = e.Name,
                        Size = e.Size,
                        LastModified = e.LastModified
                    }).ToList()
                };

                return response;
            }
        }

        public override Task<GetObjectResponse> GetObjectAsync(GetObjectRequest request, CancellationToken cancellationToken = default)
        {
            return base.GetObjectAsync(request, cancellationToken);
        }

        private static string CreateArchiveReference(string bucket, string prefix)
            => $"{bucket}/{prefix}";
    }
}