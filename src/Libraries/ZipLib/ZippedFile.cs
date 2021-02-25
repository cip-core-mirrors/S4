using System;
using System.Linq;
using System.Text;

namespace ABSA.RD.S4.ZipLib
{
    public class ZippedFile
    {
        // All magic numbers here https://en.wikipedia.org/wiki/Zip_(file_format)#File_headers
        private readonly byte[] _data;
        private readonly int _start;

        public static int LengthBeforeNameLength => NameLengthOffset - VersionOffset;
        public static int LengthAfterNameLength => NameOffset - ExtraLengthOffset;

        public int CompressedLength { get; }
        public int DecompressedLength { get; }

        public const int MagicBytesOffset = 0;
        public const int VersionOffset = MagicBytesOffset + 4;
        public const int BitFlagOffset = VersionOffset + 2;
        public const int MethodOffset = BitFlagOffset + 2;
        public const int LastModTimeOffset = MethodOffset + 2;
        public const int LastModDateOffset = LastModTimeOffset + 2;
        public const int DecompressedCrcOffset = LastModDateOffset + 2;
        public const int CompressedLengthOffset = DecompressedCrcOffset + 4;
        public const int DecompressedLengthOffset = CompressedLengthOffset + 4;
        public const int NameLengthOffset = DecompressedLengthOffset + 4;
        public const int ExtraLengthOffset = NameLengthOffset + 2;
        public const int NameOffset = ExtraLengthOffset + 2;

        public struct LocalFields
        {
            private readonly byte[] _data;
            private readonly int _start;

            public LocalFields(byte[] data, int start)
            {
                _data = data;
                _start = start;
            }

            public static byte[] MagicBytes => new byte[] { 80, 75, 3, 4 };
            public ushort Version => _data.ReadUInt16(_start + VersionOffset);
            public ushort BitFlag => _data.ReadUInt16(_start + BitFlagOffset);
            public ushort Method => _data.ReadUInt16(_start + MethodOffset);
            public ushort LastModTime => _data.ReadUInt16(_start + LastModTimeOffset);
            public ushort LastModDate => _data.ReadUInt16(_start + LastModDateOffset);
            public uint DecompressedCrc => _data.ReadUInt32(_start + DecompressedCrcOffset);
            public uint CompressedLength => _data.ReadUInt32(_start + CompressedLengthOffset);
            public uint DecompressedLength => _data.ReadUInt32(_start + DecompressedLengthOffset);
            public ushort NameLength => _data.ReadUInt16(_start + NameLengthOffset);
            public ushort ExtraLength => _data.ReadUInt16(_start + ExtraLengthOffset);
            public string Name => Encoding.UTF8.GetString(_data.AsSpan(_start + NameOffset, NameLength).ToArray());
            public ReadOnlyMemory<byte> ExtraBytes => _data.AsMemory(_start + NameOffset + NameLength, ExtraLength).ToArray();
        }

        public readonly LocalFields Fields;

        public static byte[] DescriptorMagicBytes => new byte[] { 80, 75, 7, 8 };

        public ReadOnlyMemory<byte> DataDescriptor
        {
            get
            {
                if ((Fields.BitFlag & 0x08) == 0)
                    return ReadOnlyMemory<byte>.Empty;

                const int DataDescriptorBaseLength = 12;
                var length = DataDescriptorBaseLength;
                if (DescriptorMagicBytes.SequenceEqual(_data.AsSpan(_start, DescriptorMagicBytes.Length).ToArray()))
                    length += DescriptorMagicBytes.Length;

                return _data.AsMemory(_start + NameOffset + Fields.NameLength + Fields.ExtraLength + CompressedLength, length);
            }
        }

        public int DeflateStreamOffset => NameOffset + Fields.NameLength + Fields.ExtraLength;

        public ReadOnlyMemory<byte> DeflateStream => _data.AsMemory(_start + DeflateStreamOffset, CompressedLength);
        public ReadOnlyMemory<byte> WholeHeader => _data.AsMemory(_start + VersionOffset, DeflateStreamOffset - VersionOffset);
        public ReadOnlyMemory<byte> DataBeforeNameLength => _data.AsMemory(_start + VersionOffset, LengthBeforeNameLength);

        public ZippedFile(byte[] data, int start)
        {
            _data = data;
            _start = start;

            CheckHeader();

            Fields = new LocalFields(_data, _start);

            CompressedLength = (int)Fields.CompressedLength;
            DecompressedLength = (int)Fields.DecompressedLength;

            if (CompressedLength == 0 || DecompressedLength == 0)
                throw new Exception("Lengths not present in local header");
        }

        public ZippedFile(byte[] data, int start, int compressedLen, int decompressedLen)
        {
            _data = data;
            _start = start;

            CheckHeader();

            Fields = new LocalFields(_data, _start);

            CompressedLength = compressedLen;
            DecompressedLength = decompressedLen;
        }

        private void CheckHeader()
        {
            if (!LocalFields.MagicBytes.SequenceEqual(_data.AsSpan(_start, LocalFields.MagicBytes.Length).ToArray()))
                throw new Exception("Local file header not found");
        }
    }
}
