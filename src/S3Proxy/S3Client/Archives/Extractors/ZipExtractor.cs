using ABSA.RD.S4.ZipLib;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Linq;

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

        public byte[] ExtractFile(byte[] data)
        {
            var centralDir = ConstructCentralDirectory(data);

            using var input = new MemoryStream(data.Concat(centralDir).ToArray());
            using var output = new MemoryStream();

            using var zip = new ZipFile(input) { Password = null };
            var entry = zip[0];
            zip.GetInputStream(entry).CopyTo(output);
            
            return output.ToArray();
        }

        private static byte[] ConstructCentralDirectory(byte[] data)
        {
            var nameLength = data.ReadUInt16(ZippedFile.NameLengthOffset);
            var extraLength = data.ReadUInt16(ZippedFile.ExtraLengthOffset);
            var compressedLengthTest = data.ReadUInt32(ZippedFile.CompressedLengthOffset);
            var compressedLength = data.Length - (ZippedFile.NameOffset + nameLength + extraLength);

            if (compressedLengthTest != 0 && compressedLengthTest != compressedLength)
                throw new Exception("Bad length");

            var eocdOffset = CentralDirectoryRecord.NameOffset + nameLength + extraLength;

            var cd = new byte[eocdOffset + CentralDirectory.CommentOffset];

            // offsets from https://en.wikipedia.org/wiki/ZIP_(file_format)#Local_file_header
            CentralDirectoryRecord.RecordFields.MagicBytes.CopyTo(cd, 0);
            data.AsSpan(ZippedFile.VersionOffset, 2).CopyTo(cd.AsSpan(CentralDirectoryRecord.VersionToExtractOffset, 2));
            data.AsSpan(ZippedFile.BitFlagOffset, 2).CopyTo(cd.AsSpan(CentralDirectoryRecord.BitFlagOffset, 2));
            data.AsSpan(ZippedFile.MethodOffset, 2).CopyTo(cd.AsSpan(CentralDirectoryRecord.MethodOffset, 2));
            // CRC may not be present here https://en.wikipedia.org/wiki/ZIP_(file_format)#Data_descriptor
            data.AsSpan(ZippedFile.DecompressedCrcOffset, 2).CopyTo(cd.AsSpan(CentralDirectoryRecord.DecompressedCrcOffset, 2));
            BitConverter.GetBytes((uint)compressedLength).CopyTo(cd, CentralDirectoryRecord.CompressedLengthOffset);
            data.AsSpan(ZippedFile.DecompressedLengthOffset, 4).CopyTo(cd.AsSpan(CentralDirectoryRecord.DecompressedLengthOffset, 4));
            data.AsSpan(ZippedFile.NameLengthOffset, 2).CopyTo(cd.AsSpan(CentralDirectoryRecord.NameLengthOffset, 2));
            data.AsSpan(ZippedFile.ExtraLengthOffset, 2).CopyTo(cd.AsSpan(CentralDirectoryRecord.ExtraLengthOffset, 2));
            data.AsSpan(ZippedFile.NameOffset, nameLength).CopyTo(cd.AsSpan(CentralDirectoryRecord.NameOffset, nameLength));
            // extra field contains AES encryption info https://www.winzip.com/win/en/aes_info.html
            data.AsSpan(ZippedFile.NameOffset + nameLength, extraLength).CopyTo(cd.AsSpan(CentralDirectoryRecord.NameOffset + nameLength, extraLength));

            CentralDirectory.EocdFields.MagicBytes.CopyTo(cd, eocdOffset);
            BitConverter.GetBytes((ushort)1).CopyTo(cd, eocdOffset + CentralDirectory.CdRecordCountOffset);
            BitConverter.GetBytes((ushort)1).CopyTo(cd, eocdOffset + CentralDirectory.CdTotalRecordCountOffset);
            BitConverter.GetBytes((uint)eocdOffset).CopyTo(cd, eocdOffset + CentralDirectory.CdSizeOffset);
            BitConverter.GetBytes((uint)data.Length).CopyTo(cd, eocdOffset + CentralDirectory.CdStartOffset);

            return cd;
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
