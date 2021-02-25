using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABSA.RD.S4.ZipLib
{
    public class CentralDirectory
    {
        // All magic numbers here https://en.wikipedia.org/wiki/Zip_(file_format)#File_headers
        private readonly byte[] _data;
        private readonly int _eocd;

        private readonly Lazy<List<CentralDirectoryRecord>> _records;

        public const int MagicBytesOffset = 0;
        public const int DiskNumberOffset = MagicBytesOffset + 4;
        public const int DiskNoCdStartOffset = DiskNumberOffset + 2;
        public const int CdRecordCountOffset = DiskNoCdStartOffset + 2;
        public const int CdTotalRecordCountOffset = CdRecordCountOffset + 2;
        public const int CdSizeOffset = CdTotalRecordCountOffset + 2;
        public const int CdStartOffset = CdSizeOffset + 4;
        public const int CommentLengthOffset = CdStartOffset + 4;
        public const int CommentOffset = CommentLengthOffset + 2;

        public struct EocdFields
        {
            private readonly byte[] _data;
            private readonly int _start;

            public EocdFields(byte[] data, int start)
            {
                _data = data;
                _start = start;
            }

            public static byte[] MagicBytes => new byte[] { 80, 75, 5, 6 };
            public ushort DiskNumber => _data.ReadUInt16(_start + DiskNumberOffset);
            public ushort DiskNoCdStart => _data.ReadUInt16(_start + DiskNoCdStartOffset);
            public ushort CdRecordCount => _data.ReadUInt16(_start + CdRecordCountOffset);
            public ushort CdTotalRecordCount => _data.ReadUInt16(_start + CdTotalRecordCountOffset);
            public uint CdSize => _data.ReadUInt32(_start + CdSizeOffset);
            public uint CdStart => _data.ReadUInt32(_start + CdStartOffset);
            public ushort CommentLength => _data.ReadUInt16(_start + CommentLengthOffset);
            public string Comment => Encoding.UTF8.GetString(_data.AsSpan(_start + CommentOffset, CommentLength).ToArray());
        }

        public readonly EocdFields Fields;

        public ReadOnlyMemory<byte> EndOfCentralDirectory => _data.AsMemory(_eocd + DiskNumberOffset, CommentOffset + Fields.CommentLength - DiskNumberOffset);

        public CentralDirectory(byte[] data)
        {
            _data = data;

            FindStart(out _eocd);

            Fields = new EocdFields(_data, _eocd);

            _records = new Lazy<List<CentralDirectoryRecord>>(GetRecordsImpl);
        }

        public List<CentralDirectoryRecord> Records => _records.Value;

        private List<CentralDirectoryRecord> GetRecordsImpl()
        {
            var records = new List<CentralDirectoryRecord>();

            var record = (int)Fields.CdStart;

            for (var i = 0; i < Fields.CdTotalRecordCount; ++i)
            {
                var nameLengthCd = _data.ReadUInt16(record + CentralDirectoryRecord.NameLengthOffset);
                var extraLengthCd = _data.ReadUInt16(record + CentralDirectoryRecord.ExtraLengthOffset);
                var commentLengthCd = _data.ReadUInt16(record + CentralDirectoryRecord.CommentLengthOffset);

                records.Add(new CentralDirectoryRecord(_data, record));

                record += CentralDirectoryRecord.NameOffset + nameLengthCd + extraLengthCd + commentLengthCd;
            }

            return records;
        }

        public List<ZippedFile> GetFiles()
        {
            var files = new List<ZippedFile>();

            var record = (int)Fields.CdStart;

            for (var i = 0; i < Fields.CdTotalRecordCount; ++i)
            {
                if (!CentralDirectoryRecord.RecordFields.MagicBytes.SequenceEqual(_data.AsSpan(record, CentralDirectoryRecord.RecordFields.MagicBytes.Length).ToArray()))
                    throw new Exception("Central directory header not found.");

                var compressedLen = _data.ReadUInt32(record + CentralDirectoryRecord.CompressedLengthOffset);
                var decompressedLen = _data.ReadUInt32(record + CentralDirectoryRecord.DecompressedLengthOffset);

                var nameLengthCd = _data.ReadUInt16(record + CentralDirectoryRecord.NameLengthOffset);
                var extraLengthCd = _data.ReadUInt16(record + CentralDirectoryRecord.ExtraLengthOffset);
                var commentLengthCd = _data.ReadUInt16(record + CentralDirectoryRecord.CommentLengthOffset);

                var localHeader = _data.ReadUInt32(record + CentralDirectoryRecord.LocalHeaderOffsetOffset);

                files.Add(new ZippedFile(_data, (int)localHeader, (int)compressedLen, (int)decompressedLen));

                record += CentralDirectoryRecord.NameOffset + nameLengthCd + extraLengthCd + commentLengthCd;
            }

            return files;
        }

        private void FindStart(out int eocd)
        {
            var pos = _data.Length - CommentOffset + DiskNumberOffset + 1;
            while (true)
                if (_data[--pos] == EocdFields.MagicBytes[3] && _data[--pos] == EocdFields.MagicBytes[2]
                    && _data[--pos] == EocdFields.MagicBytes[1] && _data[--pos] == EocdFields.MagicBytes[0])
                    break;

            eocd = pos;

            if (eocd + _data.ReadUInt16(eocd + CommentLengthOffset) + CommentOffset != _data.Length)
                throw new Exception("End of central directory size mismatch.");

            if (_data.ReadUInt16(eocd + CdRecordCountOffset) != _data.ReadUInt16(eocd + CdTotalRecordCountOffset))
                throw new NotSupportedException("Splitted archives not supported.");
        }
    }
}
