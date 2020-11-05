using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Electronic_journal
{
    class Program
    {
        public static readonly string[] a = { "123", "321" };
        static void Main(string[] args)
        {
            //Admin a = new Admin();
            //Settings settings = new Settings();
            Console.WriteLine(a[0]);
            a[0] = "222";
            Console.WriteLine(a[0]);

            //using (FileStream fstream = File.Open("1.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                /*fstream.Seek(1, SeekOrigin.Current);
                Console.WriteLine(fstream.Position);
                byte[] array = System.Text.Encoding.Default.GetBytes("1234");
                // запись массива байтов в файл
                fstream.Write(array, 0, array.Length);*/

                /*using (BinaryWriter writer = new BinaryWriter(fstream))
                {
                    Console.WriteLine(fstream.Position);
                    writer.Write("Ⴖ");
                    Console.WriteLine(fstream.Position);
                    writer.Write((byte)0x40);
                    Console.WriteLine(fstream.Position);
                }*/
                /*fstream.Seek(0, SeekOrigin.Begin);
                using (BinaryReader reader = new BinaryReader(fstream))while (reader.PeekChar() > -1)
                    {
                        //Console.WriteLine(reader.BaseStream.Position);
                        Console.WriteLine($"0x{Convert.ToString(reader.ReadByte(), 16).PadLeft(2, '0')}");
                        Console.WriteLine(reader.BaseStream.Position);
                    }*/
            }
            //settings.

            /*using (BinaryWriter writer = new BinaryWriter(File.Open("1.dat", FileMode.OpenOrCreate)))
            {
                writer.Write((byte)0x10);
                writer.Write(new string('1', 128*128+127));
                writer.Write((byte)0x20);
            }

            using (BinaryReader reader = new BinaryReader(File.Open("1.dat", FileMode.Open)))
            {
                Console.WriteLine(reader.ReadByte());
                Console.WriteLine(reader.ReadString().Length);
                Console.WriteLine(reader.ReadByte());
            }*/
        }
    }
}
