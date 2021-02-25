using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using ABSA.RD.S4.S3Proxy.Misc;

namespace ABSA.RD.S4.S3Proxy.Proxy
{
    class S3Proxy : IProxy
    {
        private readonly IAmazonS3 _s3;

        public S3Proxy(IAmazonS3 s3)
        {
            _s3 = s3;
        }

        public Task<ProxyResponse> MakeRequest(HttpRequest request)
        {
            var s3Request = S3Request.Parse(request);

            return s3Request.Type switch
            {
                RequestType.ListBuckets => ListBuckets(),
                RequestType.ListObjects2 => ListObjects2(s3Request),
                RequestType.GetObject => GetObject(s3Request),
                _ => throw new NotImplementedException(),
            };
        }

        private async Task<ProxyResponse> ListBuckets()
        {
            var response = await _s3.ListBucketsAsync();            
            return new S3Response(response.ToXml().ToByteArray());
        }

        private async Task<ProxyResponse> ListObjects2(S3Request s3Request)
        {
            var request = new ListObjectsV2Request
            {
                BucketName = s3Request.Bucket,
                Prefix = s3Request.Prefix,
                ContinuationToken = s3Request.ContinuationToken,
                Delimiter = s3Request.Delimeter,
                Encoding = EncodingType.FindValue(s3Request.EncodingType),
                ExpectedBucketOwner = s3Request.ExpectedBucketOwner,
                FetchOwner = s3Request.FetchOwner,
                RequestPayer = s3Request.RequestPayer,
                StartAfter = s3Request.StartAfter
            };

            // Don't assign this default value in the property initializer. It breaks aws s3 client somehow and it always return an empty response as a result.
            if (s3Request.MaxKeys != default)
                request.MaxKeys = s3Request.MaxKeys;

            var response = await _s3.ListObjectsV2Async(request);
                        
           return new S3Response(response.ToXml().ToByteArray());
        }

        private async Task<ProxyResponse> GetObject(S3Request s3Request)
        {
            var request = new GetObjectRequest
            {
                BucketName = s3Request.Bucket,
                Key = s3Request.Key,
                RequestPayer = s3Request.RequestPayer,
                ExpectedBucketOwner = s3Request.ExpectedBucketOwner,
                EtagToMatch = s3Request.IfMatch,
                EtagToNotMatch = s3Request.IfNoneMatch,
                ModifiedSinceDateUtc = s3Request.IfModifiedSince,
                UnmodifiedSinceDateUtc = s3Request.IfUnmodifiedSince,
                ServerSideEncryptionCustomerMethod = s3Request.ServerSideEncryptionCustomerAlgorithm,
                ServerSideEncryptionCustomerProvidedKey = s3Request.ServerSideEncryptionCustomerProvidedKey,
                ServerSideEncryptionCustomerProvidedKeyMD5 = s3Request.ServerSideEncryptionCustomerProvidedKeyMD5,
                ResponseExpiresUtc = s3Request.ResponseExpires,
                ByteRange = s3Request.Range,
                VersionId = s3Request.VersionId
            };

            if (s3Request.PartNumber != default)
                request.PartNumber = s3Request.PartNumber;

            var @object = await _s3.GetObjectAsync(request);
            var data = await @object.ReadData();

            return new ProxyResponse
            {
                ContentType = @object.Headers.ContentType,
                Headers = S3ApiResponseConverter.ExtractHeaders(@object),
                Body = data
            };
        }
    }
}