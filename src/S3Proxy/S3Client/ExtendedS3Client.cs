using ABSA.RD.S4.S3Proxy.Misc;
using ABSA.RD.S4.S3Proxy.S3Client.Archives;
using ABSA.RD.S4.S3Proxy.S3Client.Archives.Extractors;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
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
            var extractor = GetRequiredExtractor(request.Prefix);
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

        public override async Task<GetObjectResponse> GetObjectAsync(GetObjectRequest request, CancellationToken cancellationToken = default)
        {
            var lastIndex = request.Key.LastIndexOf('/');
            var prefix = lastIndex != -1 ? request.Key.Substring(0, lastIndex) : "";
            var key = lastIndex != -1 ? request.Key.Substring(lastIndex  + 1) : request.Key;
            byte[] deflateData;
            ArchiveInfo.ArchiveEntryInfo requestedEntry;
            GetObjectResponse response = new GetObjectResponse
            {
                BucketName = request.BucketName,
                Key = request.Key
            };

            var extractor = GetRequiredExtractor(prefix);
            if (extractor == default) // normal case, no need to use any archive extractor
                return await base.GetObjectAsync(request, cancellationToken);
            else
            {
                var archiveUniqueReference = CreateArchiveReference(request.BucketName, prefix);
                if (!_archiveInfoStorage.TryGet(archiveUniqueReference, out var info))
                {
                    // download archive from S3
                    response = await base.GetObjectAsync(new GetObjectRequest { BucketName = request.BucketName, Key = prefix }, cancellationToken);
                    var data = await response.ReadData();

                    // extract entries info and save it
                    info = extractor.ExtractInfo(data);
                    _archiveInfoStorage.Save(archiveUniqueReference, info);
                                        
                    requestedEntry = info.Entries.SingleOrDefault(x => x.Name == key);
                    if (requestedEntry == null)
                    {
                        response.HttpStatusCode = HttpStatusCode.NotFound;
                        return response;
                    }

                    deflateData = data[(int)requestedEntry.Offset..(int)(requestedEntry.Offset + requestedEntry.Length)];
                }
                else
                {
                    requestedEntry = info.Entries.SingleOrDefault(x => x.Name == key);
                    if (requestedEntry == null)
                    {
                        response.HttpStatusCode = HttpStatusCode.NotFound;
                        return response;
                    }

                    // make request and get a partial content
                    request.ByteRange = new ByteRange(requestedEntry.Offset, requestedEntry.Offset + requestedEntry.Length - 1);
                    request.Key = prefix;

                    response = await base.GetObjectAsync(request, cancellationToken);
                    deflateData = await response.ReadData();
                }

                // extract data in the original format
                var realData = extractor.ExtractFile(deflateData);
                response.ResponseStream = new MemoryStream(realData);
                response.ContentLength = realData.LongLength;
                response.Headers["Content-Length"] = realData.Length.ToString();
                response.Headers["Content-Type"] = "application/octet-stream";
                response.ETag = $"\"{requestedEntry.LastModified.Ticks:x}\"";
                response.LastModified = requestedEntry.LastModified;

                return response;
            }
        }

        private IArchiveExtractor GetRequiredExtractor(string value)
            => _extractorsMapping.SingleOrDefault(x => value.EndsWith(x.Key, StringComparison.OrdinalIgnoreCase)).Value;

        private static string CreateArchiveReference(string bucket, string prefix)
            => $"{bucket}/{prefix}";
    }
}