using ABSA.RD.S4.S3Proxy.Misc;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace ABSA.RD.S4.S3Proxy
{
    class S3Request
    {
        public static class Names
        {
            public const string ContinuationToken = "continuation-token";
            public const string RequestPayer = "x-amz-request-payer";
            public const string ExpectedBucketOwner = "x-amz-expected-bucket-owner";
            public const string ListType = "list-type";
            public const string Delimeter = "delimiter";
            public const string EncodingType = "encoding-type";
            public const string FetchOwner = "fetch-owner";
            public const string MaxKeys = "max-keys";
            public const string Prefix = "prefix";
            public const string StartAfter = "start-after";
            public const string IfMatch = "If-Match";
            public const string IfNoneMatch = "If-None-Match";
            public const string IfModifiedSince = "If-Modified-Since";
            public const string IfUnmodifiedSince = "If-Unmodified-Since";
            public const string Range = "Range";
            public const string ServerSideEncryptionCustomerAlgorithm = "x-amz-server-side-encryption-customer-algorithm";
            public const string ServerSideEncryptionCustomerProvidedKey = "x-amz-server-side-encryption-customer-key";
            public const string ServerSideEncryptionCustomerProvidedKeyMD5 = "x-amz-server-side-encryption-customer-key-MD5";
            public const string PartNumber = "partNumber";
            public const string VersionId = "versionId";
            public const string ResponseExpires = "response-expires";
        }

        public string Method { get; set; }
        public RequestType Type { get; set; }
        public string Host { get; set; }
        public string Bucket { get; set; }
        public string Prefix { get; set; }
        public string ContinuationToken { get; set; }
        public string RequestPayer { get; set; }
        public string ExpectedBucketOwner { get; set; }
        public string Delimeter { get; set; }
        public string EncodingType { get; set; }
        public bool FetchOwner { get; set; }
        public int MaxKeys { get; set; }
        public string StartAfter { get; set; }
        public string Key { get; set; }
        public string IfMatch { get; set; }
        public string IfNoneMatch { get; set; }
        public DateTime IfModifiedSince { get; set; }
        public DateTime IfUnmodifiedSince { get; set; }
        public ByteRange Range { get; set; }
        public string ServerSideEncryptionCustomerAlgorithm { get; set; }
        public string ServerSideEncryptionCustomerProvidedKey { get; set; }
        public string ServerSideEncryptionCustomerProvidedKeyMD5 { get; set; }
        public int PartNumber { get; set; }
        public DateTime ResponseExpires { get; set; }
        public string VersionId { get; set; }

        public static S3Request Parse(HttpRequest request)
        {
            var paths = request.Path.Value.Split('/', StringSplitOptions.RemoveEmptyEntries);

            var s3Request = new S3Request
            {
                Method = request.Method,
                Bucket = paths.Length > 0 ? paths[0] : null,
                Key = paths.Length > 1 ? string.Join('/', paths.Skip(1)) : null,
                ContinuationToken = request.Query.GetValue(Names.ContinuationToken),
                Delimeter = request.Query.GetValue(Names.Delimeter),
                EncodingType = request.Query.GetValue(Names.EncodingType),
                ExpectedBucketOwner = request.Headers.GetValue(Names.ExpectedBucketOwner),
                FetchOwner = bool.TryParse(request.Query.GetValue(Names.FetchOwner), out var fetchOwner) && fetchOwner,
                MaxKeys = int.TryParse(request.Query.GetValue(Names.MaxKeys), out var maxKeys) ? maxKeys : default,
                Prefix = request.Query.GetValue(Names.Prefix),
                RequestPayer = request.Headers.GetValue(Names.RequestPayer),
                StartAfter = request.Query.GetValue(Names.StartAfter),
                IfMatch = request.Headers.GetValue(Names.IfMatch),
                IfNoneMatch = request.Headers.GetValue(Names.IfNoneMatch),
                IfModifiedSince = DateTime.TryParse(request.Headers.GetValue(Names.IfModifiedSince), out var modifiedSince) ? modifiedSince : default,
                IfUnmodifiedSince = DateTime.TryParse(request.Headers.GetValue(Names.IfUnmodifiedSince), out var unmodifiedSince) ? unmodifiedSince : default,
                Range = !string.IsNullOrEmpty(request.Headers.GetValue(Names.IfNoneMatch)) ? new ByteRange(request.Headers.GetValue(Names.IfNoneMatch)) : default,
                ServerSideEncryptionCustomerAlgorithm = request.Headers.GetValue(Names.ServerSideEncryptionCustomerAlgorithm),
                ServerSideEncryptionCustomerProvidedKey = request.Headers.GetValue(Names.ServerSideEncryptionCustomerProvidedKey),
                ServerSideEncryptionCustomerProvidedKeyMD5 = request.Headers.GetValue(Names.ServerSideEncryptionCustomerProvidedKeyMD5),
                PartNumber = int.TryParse(request.Query.GetValue(Names.PartNumber), out var partNumber) ? partNumber : default,
                ResponseExpires = DateTime.TryParse(request.Query.GetValue(Names.ResponseExpires), out var responseExpire) ? responseExpire : default,
                VersionId = request.Query.GetValue(Names.VersionId)
            };

            s3Request.Type = GetRequestType(request, s3Request);

            return s3Request;
        }

        private static RequestType GetRequestType(HttpRequest request, S3Request s3Request)
        {
            if (request.Path == "/" && request.Method == "GET")
                return RequestType.ListBuckets;

            if (request.Query.ContainsKey(Names.ListType))
                return RequestType.ListObjects2;

            if (request.Method == "GET" && !string.IsNullOrEmpty(s3Request.Bucket) && !string.IsNullOrEmpty(s3Request.Key))
                return RequestType.GetObject;

            return RequestType.Undefined;
        }
    }

    enum RequestType
    {
        Undefined = -1,

        ListBuckets,
        ListObjects2,
        GetObject
    }
}