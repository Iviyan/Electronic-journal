using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Electronic_journal
{
    public static class Extensions
    {
        /*public static int Read7BitEncodedInt(this System.IO.BinaryReader reader)
        {
            sbyte b;
            int r = -7, v = 0;
            do
                v |= ((b = reader.ReadSByte()) & 0x7F) << (r += 7);
            while (b < 0);
            return v;
        }*/

        public static bool Contains(this string[] array, string value)
        {
            if (array == null) return false;

            for (int i = 0; i < array.Length; i++)
                if (array[i] == value)
                    return true;

            return false;
        }
        public static bool ChangeKey<TKey, TValue>(this IDictionary<TKey, TValue> dict,
                                           TKey oldKey, TKey newKey)
        {
            TValue value;
            if (!dict.Remove(oldKey, out value))
                return false;

            dict[newKey] = value;
            return true;
        }

        public static int Read7BitEncodedInt(this System.IO.BinaryReader reader)
        {
            uint result = 0;
            byte byteReadJustNow;
            const int MaxBytesWithoutOverflow = 4;

            for (int shift = 0; shift < MaxBytesWithoutOverflow * 7; shift += 7)
            {
                byteReadJustNow = reader.ReadByte();
                result |= (byteReadJustNow & 0x7Fu) << shift;

                if (byteReadJustNow <= 0x7Fu)
                    return (int)result;
            }
            byteReadJustNow = reader.ReadByte();
            if (byteReadJustNow > 0b_1111u)
                throw new FormatException("Bad7BitInt");

            result |= (uint)byteReadJustNow << (MaxBytesWithoutOverflow * 7);
            return (int)result;
        }
        public static void Write7BitEncodedInt(this System.IO.BinaryWriter writer, int value)
        {
            uint uValue = (uint)value;
            while (uValue > 0x7Fu)
            {
                writer.Write((byte)(uValue | ~0x7Fu));
                uValue >>= 7;
            }

            writer.Write((byte)uValue);
        }
        public static void Write(this BinaryWriter writer, string[] array)
        {
            writer.Write7BitEncodedInt(array.Length);
            for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
        }
        public static string[] ReadStringArray(this BinaryReader reader)
        {
            int length = reader.Read7BitEncodedInt();
            string[] result = new string[length];
            for (int i = 0; i < length; i++) result[i] = reader.ReadString();
            return result;
        }
        public static void SkipString(this BinaryReader reader) => reader.BaseStream.Position += reader.Read7BitEncodedInt();

        public static void Write(this BinaryWriter writer, DateTime date) => writer.Write(date.ToBinary());
        public static DateTime ReadDateTime(this BinaryReader reader) => DateTime.FromBinary(reader.ReadInt64());
    }
}
//https://source.dot.net/#System.Private.CoreLib/BinaryWriter.cs,cf806b417abe1a35