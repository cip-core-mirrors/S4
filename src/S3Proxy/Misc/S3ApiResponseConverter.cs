using Amazon.S3.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ABSA.RD.S4.S3Proxy.Misc
{
    /// <summary>
    /// Defines a common operation for converting, extracting and transformation S3 api responses
    /// </summary>
    public static class S3ApiResponseConverter
    {
        public static XElement ToXml(this ListBucketsResponse response)
        {
            var buckets =
                from b in response.Buckets
                select new XElement("Bucket",
                    new XElement("CreationDate", b.CreationDate.ToString("yyyy-MM-dd HH:mm:ss")),
                    new XElement("Name", b.BucketName));

            return new XElement("ListAllMyBucketsResult",
                    new XElement("Buckets", buckets));
        }
        
        public static XElement ToXml(this ListObjectsV2Response response)
        {
            var contents = new XElement[response.S3Objects.Count];

            // contents
            for (var i = 0; i < response.S3Objects.Count; i++)
            {
                var o = response.S3Objects[i];

                var el = new XElement("Contents",
                    new XElement("ETag", o.ETag),
                    new XElement("Key", o.Key),
                    new XElement("LastModified", o.LastModified.ToString("yyyy-MM-dd HH:mm:ss")),
                    new XElement("Size", o.Size),
                    new XElement("StorageClass", o.StorageClass));

                if (o.Owner != null)
                {
                    var owner = new XElement("Owner",
                        new XElement("DisplayName", o.Owner.DisplayName),
                        new XElement("ID", o.Owner.Id));

                    el.Add(owner);
                }

                contents[i] = el;
            }

            // common prefixes
            var commonPrefixes = new XElement[response.CommonPrefixes.Count];
            for (int i = 0; i < response.CommonPrefixes.Count; i++)
            {
                var prefix = new XElement("CommonPrefixes",
                    new XElement("Prefix", response.CommonPrefixes[i]));

                commonPrefixes[i] = prefix;
            }

            var result = new XElement("ListBucketResult",
                        new XElement("IsTruncated", response.IsTruncated),
                        new XElement("Name", response.Name),
                        new XElement("MaxKeys", response.MaxKeys),
                        new XElement("KeyCount", response.KeyCount));

            if (response.Encoding != default)
                result.Add("EncodingType", response.Encoding);

            if (response.NextContinuationToken != default)
                result.Add("NextContinuationToken", response.NextContinuationToken);

            if (response.ContinuationToken != default)
                result.Add("ContinuationToken", response.ContinuationToken);

            if (response.StartAfter != default)
                result.Add("StartAfter", response.StartAfter);

            if (commonPrefixes.Length > 0)
                result.Add(commonPrefixes);

            if (contents.Length > 0)
                result.Add(contents);

            return result;
        }

        public static Dictionary<string, string> ExtractHeaders(GetObjectResponse response)
        {
            var headers = response.Headers.Keys.ToDictionary(x => x, x => response.Headers[x]);

            headers.TryAdd(S3Response.Names.ContentLength, response.ContentLength.ToString());
            headers.TryAdd(S3Response.Names.BucketKeyEnabled, response.BucketKeyEnabled.ToString());
            
            if (response.TagCount != default)
                headers.TryAdd(S3Response.Names.TagCount, response.TagCount.ToString());

            if (!string.IsNullOrEmpty(response.AcceptRanges))
                headers.TryAdd(S3Response.Names.AcceptRanges, response.AcceptRanges);

            if (!string.IsNullOrEmpty(response.ContentRange))
                headers.TryAdd(S3Response.Names.ContentRange, response.ContentRange);

            if (!string.IsNullOrEmpty(response.DeleteMarker))
                headers.TryAdd(S3Response.Names.DeleteMarker, response.DeleteMarker);

            if (!string.IsNullOrEmpty(response.ETag))
                headers.TryAdd(S3Response.Names.ETag, response.ETag);

            if (response.Expires != default)
                headers.TryAdd(S3Response.Names.Expiration, response.Expires.ToString("yyyy-MM-dd HH:mm:ss"));
                        
            if (response.LastModified != default)
                headers.TryAdd(S3Response.Names.LastModified, response.LastModified.ToString("yyyy-MM-dd HH:mm:ss"));

            if (!string.IsNullOrEmpty(response.ObjectLockLegalHoldStatus?.Value))
                headers.TryAdd(S3Response.Names.ObjectLockLegalHoldStatus, response.ObjectLockLegalHoldStatus.Value);

            if (!string.IsNullOrEmpty(response.ObjectLockMode?.Value))
                headers.TryAdd(S3Response.Names.ObjectLockMode, response.ObjectLockMode.Value);

            if (response.ObjectLockRetainUntilDate != default)
                headers.TryAdd(S3Response.Names.ObjectLockRetainUntilDate, response.ObjectLockRetainUntilDate.ToString("yyyy-MM-dd HH:mm:ss"));

            if (response.PartsCount.HasValue)
                headers.TryAdd(S3Response.Names.PartsCount, response.PartsCount.Value.ToString());

            if (!string.IsNullOrEmpty(response.ReplicationStatus?.Value))
                headers.TryAdd(S3Response.Names.ReplicationStatus, response.ReplicationStatus.Value);

            if (!string.IsNullOrEmpty(response.RequestCharged?.Value))
                headers.TryAdd(S3Response.Names.RequestCharged, response.RequestCharged.Value);

            if (!string.IsNullOrEmpty(response.StorageClass?.Value))
                headers.TryAdd(S3Response.Names.StorageClass, response.StorageClass.Value);

            if (!string.IsNullOrEmpty(response.ServerSideEncryptionCustomerMethod?.Value))
                headers.TryAdd(S3Response.Names.SSECustomerAlgorithm, response.ServerSideEncryptionCustomerMethod.Value);

            if (!string.IsNullOrEmpty(response.ServerSideEncryptionKeyManagementServiceKeyId))
                headers.TryAdd(S3Response.Names.SSEKMSKeyId, response.ServerSideEncryptionKeyManagementServiceKeyId);

            if (!string.IsNullOrEmpty(response.ServerSideEncryptionMethod?.Value))
                headers.TryAdd(S3Response.Names.ServerSideEncryption, response.ServerSideEncryptionMethod.Value);

            if(!string.IsNullOrEmpty(response.VersionId))
                headers.TryAdd(S3Response.Names.VersionId, response.VersionId);

            if (!string.IsNullOrEmpty(response.WebsiteRedirectLocation))
                headers.TryAdd(S3Response.Names.WebsiteRedirectLocation, response.WebsiteRedirectLocation);

            if (!string.IsNullOrEmpty(response.Headers.CacheControl))
                headers.TryAdd(S3Response.Names.CacheControl, response.Headers.CacheControl);

            if (!string.IsNullOrEmpty(response.Headers.ContentDisposition))
                headers.TryAdd(S3Response.Names.ContentDisposition, response.Headers.ContentDisposition);

            if (!string.IsNullOrEmpty(response.Headers.ContentEncoding))
                headers.TryAdd(S3Response.Names.ContentEncoding, response.Headers.ContentEncoding);

            return headers;
        }

        public static async Task<byte[]> ReadData(this GetObjectResponse response)
        {
            var data = new byte[response.ContentLength];
            var start = 0;
            while (start < data.Length)
                start += await response.ResponseStream.ReadAsync(data, start, data.Length - start);

            return data;
        }
    }
}