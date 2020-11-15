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

            /*StringArrayEditor sa = new(new string[] { "123456789", "987654321", "4534553453453453", "4534553453453453", "4534553453453453", "4534553453453453", "4534553453453453", "4534553453453453", "4534553453453453", }, "Title", 4);
            sa.Edit();
            Helper.mb(Helper.ArrayToStr(sa.GetArray().Select(s => $"|{s}|").ToArray()));*/

            
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
            /*var t = new Teacher("", "", "", "", "", DateTime.Today, Array.Empty<string>(), new string[] { "123456789","987654321", "4534553453453453", "4534553453453453", "4534553453453453", "4534553453453453", "4534553453453453", "4534553453453453", "4534553453453453", });
            new ClassEditor<Teacher>(t,
                (Teacher ab_, out string msg) =>
                {
                    msg = ""; return true;
                }
                ).Edit();

            return;*/

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

            Dictionary<string, int> accountsTable;

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
                accountsTable = settings.GetAccountsList();
                accountsMenu = accountsTable.Keys.ToArray();
                menu.Update(accountsMenu);
                int sel = menu.Choice(
                    (ConsoleKeyInfo key, int selectedIndex) =>
                    {
                        if (key.Key == ConsoleKey.Escape) return -1;
                        return null;
                    }
                );
                if (sel == -1) ShowMainMenu(from: 1);
                int pos = accountsTable[accountsMenu[sel]];
                ShowAccountEditMenu(pos);
            }
            void ShowAccountEditMenu(int posInFile)
            {
                //Helper.mb(posInFile);
                Account account = settings.LoadAccount(posInFile);
                menu.Clear();

                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(stream);
                
                switch (account.Type)
                {
                    case Account.AccountType.Admin:
                        ClassEditor<Admin> editor = new ClassEditor<Admin>(
                            (Admin)account,
                            (Admin admin, out string msg) =>
                            {
                                msg = ""; return true;
                            }
                        );
                        editor.Edit();
                        account.Export(writer);
                        break;
                }
                Helper.mb(Helper.ArrayToStr(stream.ToArray()));
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
