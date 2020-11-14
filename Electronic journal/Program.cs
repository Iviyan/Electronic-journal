using System;
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
            //Admin a = new Admin();

            //Reader.ReadLine_esc();

            StringArrayEditor sa = new(new[] { "str", "str2", "..." }, "Title", 0);
            sa.Edit();

            Console.Read();
            Settings settings = new Settings();

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
            var st = new Student("", "", "", "", "", DateTime.Today, "");
            new ClassEditor<Student>(st,
                (Student ab_, out string msg) =>
                {
                    msg = ""; return true;
                }
                ).Edit();

            return;

            string[] mainMenu = new string[]
                {
                    "Группы",
                    "Аккаунты",
                    "Добавить аккаунт",
                    "Добавить группу",
                    "Сменить пароль"
                };
            string[] groupsMenu;
            string[] accountsMenu;

            ConsoleSelect menu = new ConsoleSelect(
                mainMenu
            );
            void ShowMainMenu(bool updateMenu = true, int from = 0)
            {
                if (updateMenu) menu.Update(mainMenu);
                switch (menu.Choice(from))
                {
                    case 0:
                        break;
                    case 1:
                        ShowAccountsMenu();
                        break;
                    case 2:
                        ShowAccountsMenu();
                        break;
                }
            }
            void ShowAccountsMenu()
            {
                accountsMenu = settings.GetAccountsList().Keys.ToArray();
                menu.Update(accountsMenu);
                int sel = menu.Choice(
                    (ConsoleKeyInfo key, int selectedIndex) =>
                    {
                        if (key.Key == ConsoleKey.Escape)
                        {
                            ShowMainMenu(from: 1);
                        };
                        return false;
                    }
                );

            }
            void ShowAccountEditMenu(int posInFile)
            {
                Account account = settings.LoadAccount(posInFile);
                menu.Clear();

            }
            ShowMainMenu(false);
            return;
            //Console.WriteLine((int)Math.Ceiling("1234567".Length / (double)3));

            //Settings settings = new Settings();

            Console.WriteLine("Авторизация:");
            Console.Write("Логин: ");
            string login = Console.ReadLine();
            Console.Write("Пароль: ");
            string pass = Console.ReadLine();

            Account a = settings.TryLogin(login, pass);
            Console.WriteLine(a.Type);

            Console.ReadKey();
            Console.Clear();

            ((IUI)a).UI();
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
