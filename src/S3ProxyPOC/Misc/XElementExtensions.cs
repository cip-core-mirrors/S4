using System.IO;
using System.Xml.Linq;

namespace ABSA.RD.S4.S3Proxy.Misc
{
    public static class XElementExtensions
    {
        public static byte[] ToByteArray(this XElement element)
        {
            using var ms = new MemoryStream();
            element.Save(ms, SaveOptions.DisableFormatting);
            return ms.ToArray();
        }
    }
}