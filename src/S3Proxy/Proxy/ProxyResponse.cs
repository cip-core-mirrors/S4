using System.Collections.Generic;

namespace ABSA.RD.S4.S3Proxy.Proxy
{
    public class ProxyResponse
    {
        public string ContentType { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public byte[] Body { get; set; }
    }
}