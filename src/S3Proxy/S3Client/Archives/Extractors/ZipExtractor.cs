using ABSA.RD.S4.ZipLib;
using System;

namespace ABSA.RD.S4.S3Proxy.S3Client.Archives.Extractors
{
    class ZipExtractor : IArchiveExtractor
    {
        /// <summary>
        /// The default DOS time. 
        /// <remark>Actually, it is a minimun DOS date.</remark>
        /// </summary>
        private static readonly DateTime _defaultDosTime = new DateTime(1980, 1, 1);

        public ArchiveInfo ExtractInfo(byte[] data)
        {
            var centralDir = new CentralDirectory(data);
            var entriesInfo = new ArchiveInfo.ArchiveEntryInfo[centralDir.Records.Count];

            for (var i = 0; i < centralDir.Records.Count; i++)
            {
                var record = centralDir.Records[i];
                var entryInfo = new ArchiveInfo.ArchiveEntryInfo
                {
                    Name = record.Fields.Name,
                    LastModified = ConvertsDOSTimeToDateTime(record.Fields.LastModified),
                    Offset = record.Fields.LocalHeaderOffset,
                    Size = record.Fields.DecompressedLength,
                    Length =
                        i == centralDir.Records.Count - 1
                        ? centralDir.Fields.CdStart - record.Fields.LocalHeaderOffset
                        : centralDir.Records[i + 1].Fields.LocalHeaderOffset - record.Fields.LocalHeaderOffset
                };

                entriesInfo[i] = entryInfo;
            }

            return new ArchiveInfo
            {
                Entries = entriesInfo
            };
        }

        /// <summary>
        /// Convert DOS time to the .NET date.
        /// </summary>
        /// <param name="dateTime">DOS date</param>
        /// <returns>.NET date</returns>
        private static DateTime ConvertsDOSTimeToDateTime(uint dateTime)
        {
            // invalid input date, return a default DOS date.
            if (dateTime <= 0)
                return _defaultDosTime;

            // DOS Time format specification.
            // 32 bits
            // Year: 7 bits, where 0 is 1980.
            // Month: 4 bits
            // Day: 5 bits
            // Hour: 5
            // Minute: 6 bits
            // Second: 5 bits

            var year = (int)(1980 + (dateTime >> 25));
            var month = (int)((dateTime >> 21) & 0xF);
            var day = (int)((dateTime >> 16) & 0x1F);
            var hour = (int)((dateTime >> 11) & 0x1F);
            var minute = (int)((dateTime >> 5) & 0x3F);
            var second = (int)((dateTime & 0x001F) * 2);

            try
            {
                return new DateTime(year, month, day, hour, minute, second, 0);
            }
            catch
            {
                return _defaultDosTime;
            }
        }
    }
}
