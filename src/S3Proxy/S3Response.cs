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
using ABSA.RD.S4.S3Proxy.Proxy;

namespace ABSA.RD.S4.S3Proxy
{
    class S3Response : ProxyResponse
    {
        public static class Names
        {
            public const string DeleteMarker = "x-amz-delete-marker";
            public const string AcceptRanges = "accept-ranges";
            public const string Expiration = "x-amz-expiration";
            public const string Restore = "x-amz-restore";
            public const string LastModified = "Last-Modified";
            public const string ContentLength = "Content-Length";
            public const string ETag = "ETag";
            public const string MissingMeta = "x-amz-missing-meta";
            public const string VersionId = "x-amz-version-id";
            public const string CacheControl = "Cache-Control";
            public const string ContentDisposition = "Content-Disposition";
            public const string ContentEncoding = "Content-Encoding";
            public const string ContentLanguage = "Content-Language";
            public const string ContentRange = "Content-Range";
            public const string ContentType = "Content-Type";
            public const string Expires = "Expires";
            public const string WebsiteRedirectLocation = "x-amz-website-redirect-location";
            public const string ServerSideEncryption = "x-amz-server-side-encryption";
            public const string SSECustomerAlgorithm = "x-amz-server-side-encryption-customer-algorithm";
            public const string SSECustomerKeyMD5 = "x-amz-server-side-encryption-customer-key-MD5";
            public const string SSEKMSKeyId = "x-amz-server-side-encryption-aws-kms-key-id";
            public const string BucketKeyEnabled = "x-amz-server-side-encryption-bucket-key-enabled";
            public const string StorageClass = "x-amz-storage-class";
            public const string RequestCharged = "x-amz-request-charged";
            public const string ReplicationStatus = "x-amz-replication-status";
            public const string PartsCount = "x-amz-mp-parts-count";
            public const string TagCount = "x-amz-tagging-count";
            public const string ObjectLockMode = "x-amz-object-lock-mode";
            public const string ObjectLockRetainUntilDate = "x-amz-object-lock-retain-until-date";
            public const string ObjectLockLegalHoldStatus = "x-amz-object-lock-legal-hold";
        }

        public S3Response()
        {
            ContentType = "application/xml";
        }

        public S3Response(byte[] body)
            : this()
        {
            Body = body;
        }
    }
}