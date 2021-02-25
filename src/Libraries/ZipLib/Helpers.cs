using System;

namespace ABSA.RD.S4.ZipLib
{
    public static class Helpers
    {
        public static ushort ReadUInt16(this byte[] bytes, int start)
            => BitConverter.ToUInt16(bytes.AsSpan(start, 2));

        public static uint ReadUInt32(this byte[] bytes, int start)
            => BitConverter.ToUInt32(bytes.AsSpan(start, 4));
    }
}