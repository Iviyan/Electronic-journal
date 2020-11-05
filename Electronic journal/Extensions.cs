using System;
using System.Collections.Generic;
using System.Text;

namespace Electronic_journal
{
    public static class Extensions
    {
        public static int Read7BitEncodedInt(this System.IO.BinaryReader reader)
        {
            sbyte b;
            int r = -7, v = 0;
            do
                v |= ((b = reader.ReadSByte()) & 0x7F) << (r += 7);
            while (b < 0);
            return v;
        }
        public static void SkipString(this System.IO.BinaryReader reader) => reader.BaseStream.Position += reader.Read7BitEncodedInt();

        public static void Write(this System.IO.BinaryWriter writer, DateTime date) => writer.Write(date.ToBinary());
        public static DateTime ReadDateTime(this System.IO.BinaryReader reader) => DateTime.FromBinary(reader.ReadInt64());
    }
}
