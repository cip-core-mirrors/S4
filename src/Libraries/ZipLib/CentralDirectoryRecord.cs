using System;
using System.Linq;
using System.Text;

namespace ABSA.RD.S4.ZipLib
{
    public class CentralDirectoryRecord
    {
        // All magic numbers here https://en.wikipedia.org/wiki/Zip_(file_format)#File_headers
        private readonly byte[] _data;
        private readonly int _start;

        public static int LengthBeforeNameLength => NameLengthOffset - VersionMadeByOffset;
        public static int LengthAfterNameLength => NameOffset - ExtraLengthOffset;

        public const int MagicBytesOffset = 0;
        public const int VersionMadeByOffset = MagicBytesOffset + 4;
        public const int VersionToExtractOffset = VersionMadeByOffset + 2;
        public const int BitFlagOffset = VersionToExtractOffset + 2;
        public const int MethodOffset = BitFlagOffset + 2;
        public const int LastModTimeOffset = MethodOffset + 2;
        public const int LastModDateOffset = LastModTimeOffset + 2;
        public const int DecompressedCrcOffset = LastModDateOffset + 2;
        public const int CompressedLengthOffset = DecompressedCrcOffset + 4;
        public const int DecompressedLengthOffset = CompressedLengthOffset + 4;
        public const int NameLengthOffset = DecompressedLengthOffset + 4;
        public const int ExtraLengthOffset = NameLengthOffset + 2;
        public const int CommentLengthOffset = ExtraLengthOffset + 2;
        public const int DiskNumberOffset = CommentLengthOffset + 2;
        public const int InternalFileAttribOffset = DiskNumberOffset + 2;
        public const int ExternalFileAttribOffset = InternalFileAttribOffset + 2;
        public const int LocalHeaderOffsetOffset = ExternalFileAttribOffset + 4;
        public const int NameOffset = LocalHeaderOffsetOffset + 4;

        public struct RecordFields
        {
            private readonly byte[] _data;
            private readonly int _start;

            public RecordFields(byte[] data, int start)
            {
                _data = data;
                _start = start;
            }

            public static byte[] MagicBytes => new byte[] { 80, 75, 1, 2 };
            public ushort VersionMadeBy => _data.ReadUInt16(_start + VersionMadeByOffset);
            public ushort VersionToExtract => _data.ReadUInt16(_start + VersionToExtractOffset);
            public ushort BitFlag => _data.ReadUInt16(_start + BitFlagOffset);
            public ushort Method => _data.ReadUInt16(_start + MethodOffset);
            public uint LastModified => _data.ReadUInt32(_start + LastModTimeOffset);
            public uint DecompressedCrc => _data.ReadUInt32(_start + DecompressedCrcOffset);
            public uint CompressedLength => _data.ReadUInt32(_start + CompressedLengthOffset);
            public uint DecompressedLength => _data.ReadUInt32(_start + DecompressedLengthOffset);
            public ushort NameLength => _data.ReadUInt16(_start + NameLengthOffset);
            public ushort ExtraLength => _data.ReadUInt16(_start + ExtraLengthOffset);
            public ushort CommentLength => _data.ReadUInt16(_start + CommentLengthOffset);
            public ushort DiskNumber => _data.ReadUInt16(_start + DiskNumberOffset);
            public ushort InternalFileAttrib => _data.ReadUInt16(_start + InternalFileAttribOffset);
            public uint ExternalFileAttrib => _data.ReadUInt32(_start + ExternalFileAttribOffset);
            public uint LocalHeaderOffset => _data.ReadUInt32(_start + LocalHeaderOffsetOffset);
            public string Name => Encoding.UTF8.GetString(_data.AsSpan(_start + NameOffset, NameLength).ToArray());
            public ReadOnlyMemory<byte> ExtraBytes => _data.AsSpan(_start + NameOffset + NameLength, ExtraLength).ToArray();
            public string Comment => Encoding.UTF8.GetString(_data.AsSpan(_start + NameOffset + NameLength + ExtraLength, CommentLength).ToArray());
        }

        public readonly RecordFields Fields;

        public ReadOnlyMemory<byte> WholeRecord => _data.AsMemory(_start + VersionMadeByOffset, NameOffset + Fields.NameLength + Fields.ExtraLength + Fields.CommentLength - VersionMadeByOffset);
        public ReadOnlyMemory<byte> DataBeforeNameLength => _data.AsMemory(_start + VersionMadeByOffset, LengthBeforeNameLength);
        public ReadOnlyMemory<byte> DataAfterNameLength => _data.AsMemory(_start + ExtraLengthOffset, LengthAfterNameLength);
        public ReadOnlyMemory<byte> DataAfterName => _data.AsMemory(_start + NameOffset + Fields.NameLength, Fields.ExtraLength + Fields.CommentLength);

        public CentralDirectoryRecord(byte[] data, int start)
        {
            if (!RecordFields.MagicBytes.SequenceEqual(data.AsSpan(start, RecordFields.MagicBytes.Length).ToArray()))
                throw new Exception("Local file header not found");

            _data = data;
            _start = start;

            Fields = new RecordFields(_data, _start);
        }

        public ZippedFile GetFile()
            => new ZippedFile(_data, (int)Fields.LocalHeaderOffset, (int)Fields.CompressedLength, (int)Fields.DecompressedLength);
    }
}
