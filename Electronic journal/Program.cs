using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace Electronic_journal
{
    class Program
    {
        /*public class AB
        {
            [Editor("AAA")]
            public int a { get; set; }
            [Editor("BBBBB")]
            public int b { get; set; }
            [Editor("DD")]
            [DateTimeMode(OnlyDate = true)]
            public DateTime d { get; set; }
        }*/
        static void Main(string[] args)
        {
            Admin ad = new Admin("admin", "admin");
            //if (1 is >= 0 and <= 3)

            /*Group gr = new Group("П50-1-19");
            gr.Teachers.Add("t1");
            gr.Teachers.Add("t2");
            gr.Disciplines.Add("d1");
            gr.Disciplines.Insert(0, "d0");
            gr.AddDisciplineForTeacher(0, 1);
            gr.Students.Add("st1");
            gr.Students.Add("st2");
            gr.AddMark(0, 0, 5, DateTime.Today);*/
            //Helper.mb(typeof(int[]).IsValueType);

            /*(gr.Teachers as IList<string>).Add("t1");
            string[] s = new string[] { "123" };
            Helper.mb((s as IList<string>).IsReadOnly);//.Add("321");
            Helper.mb(Helper.ArrayToStr(s));*/

            //Reader.ReadLine_esc();

            /*StringArrayEditor sa = new(new string[] { "123456789", "987654321", "4534553453453453", "4534553453453453", "4534553453453453", "4534553453453453", "4534553453453453", "4534553453453453", "4534553453453453", }, "Title", 4);
            sa.Edit();
            Helper.mb(Helper.ArrayToStr(sa.GetArray().Select(s => $"|{s}|").ToArray()));*/
            //Helper.mb('|', String.Join(',', "1 2 3 ".Split(' ', StringSplitOptions.RemoveEmptyEntries)), '|');

            Settings settings = new Settings();
            //gr.Export(settings);
            //settings.RemoveAccount("Ivan_V");
            /*Student st = new Student("Ivan_V", "12345", "Иван", "Воркунов", "Викторович", DateTime.Today, "П50-1-19");
            var w = settings.GetAccountFileWriter("IV", FileMode.Create);
            st.Export(w);
            w.Dispose();
           var r = settings.GetAccountFileReader("Ivan_V");
           r.ReadByte();
           Student st1 = new Student(r);
           Helper.mb(st1.Group);*/


            ad.UI(settings);
            Console.Read();
            /*AB ab = new AB();
            ab.a = 123;
            new ClassEditor<AB>(ab,
                (AB ab_, out string msg) =>
                {
                    if (ab_.b <= 0)
                    {
                        msg = "B должна быть больше 0";
                        return false;
                    }
                    msg = ""; return true;
                }
                ).Edit();
            Helper.mb(ab.a, " ", ab.b, " ", ab.d);
            Console.Read();*/
            /*var t = new Teacher("", "", "", "", "", DateTime.Today, Array.Empty<string>(), new string[] { "123456789","987654321", "4534553453453453", "4534553453453453", "4534553453453453", "4534553453453453", "4534553453453453", "4534553453453453", "4534553453453453", });
            new ClassEditor<Teacher>(t,
                (Teacher ab_, out string msg) =>
                {
                    msg = ""; return true;
                }
                ).Edit();

            return;*/

            
            return;
            //Console.WriteLine((int)Math.Ceiling("1234567".Length / (double)3));

            //Settings settings = new Settings();

            Console.WriteLine("Авторизация:");
            Console.Write("Логин: ");
            string login = Console.ReadLine();
            Console.Write("Пароль: ");
            string pass = Console.ReadLine();

            // TODO: Сделать цикл
            Account a = settings.TryLogin(login, pass, out string errorMsg);
            Console.WriteLine(a.Type);

            Console.ReadKey();
            Console.Clear();

            ((IUI)a).UI(settings);
            /*switch (a.Type)
            {
                case Account.AccountType.Admin: 
            }*/


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
