using System;

namespace ABSA.RD.S4.S3Proxy.S3Client.Archives
{
    class ArchiveInfo
    {
        public ArchiveEntryInfo[] Entries { get; set; }

        public class ArchiveEntryInfo
        {
            public string Name { get; set; }
            public uint Offset { get; set; }
            public uint Length { get; set; }
            public uint Size { get; set; }
            public DateTime LastModified { get; set; }
        }
    }
}