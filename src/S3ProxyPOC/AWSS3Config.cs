namespace ABSA.RD.S3Proxy
{
    public class S3Config
    {
        public string Region { get; set; }
        public string Bucket { get; set; }
        public string Prefix { get; set; }
        public string KmsKey { get; set; }

        public string CredentialsProfile { get; set; }

        public bool NoProxy { get; set; }
        public int TimeoutSeconds { get; set; }
    }
}