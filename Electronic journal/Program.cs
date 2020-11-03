using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Electronic_journal
{
    class Program
    {
        static void Main(string[] args)
        {
            //Admin a = new Admin("","");
            Settings settings = new Settings();
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
