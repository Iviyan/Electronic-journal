using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Electronic_journal
{
    public class Settings
    {
        public string FolderName { get; }
        public string GroupsFolderName { get; }
        public SettingsFile AccountsSettingsFile { get; }
        public SettingsFile GroupsSettingsFile { get; }

        public const string DefaultAdminLogin = "admin";
        public const string DefaultAdminPassword = "admin";

        private List<string> subjects = new List<string>();
        public string[] Subjects { get => subjects.ToArray(); }

        public Settings(
            string folderName = "Electronic Journal",
            string groupsFolderName = "Groups",
            string accountsFileName = "Accounts.dat",
            string groupsFileName = "Groups.dat"
            )
        {
            FolderName = folderName;
            GroupsFolderName = $@"{folderName}\{groupsFolderName}";
            AccountsSettingsFile = new SettingsFile(accountsFileName, folderName);
            GroupsSettingsFile = new SettingsFile(groupsFileName, folderName);

            if (!Directory.Exists(FolderName)) Directory.CreateDirectory(FolderName);
            if (!Directory.Exists(GroupsFolderName)) Directory.CreateDirectory(GroupsFolderName);

            if (!AccountsSettingsFile.Exists)
                AddAccount(new Admin(DefaultAdminLogin, DefaultAdminPassword));

        }

        public void AddAccount(Account account)
        {
            using (BinaryWriter writer = AccountsSettingsFile.GetFileWriter())
            {
                writer.BaseStream.Position += 2;
                long startPosition = writer.BaseStream.Position;
                account.Export(writer);
                long length = writer.BaseStream.Position - startPosition;
                writer.BaseStream.Position = 0;
                writer.Write((ushort)length);
            }
        }

        public Account LoadAccount(int position)
        {
            BinaryReader reader = AccountsSettingsFile.GetFileReader();
            reader.BaseStream.Position = position;
            return LoadAccount(reader);
        }
        public Account LoadAccount(BinaryReader reader) => LoadAccount(reader, reader.ReadString(), reader.ReadString());
        public Account LoadAccount(BinaryReader reader, string login, string pass)
        {
            Account.AccountType type = (Account.AccountType)reader.ReadByte();
            switch (type)
            {
                case Account.AccountType.Admin: return new Admin(login, pass);
                case Account.AccountType.Teacher: return new Teacher(login, pass, reader);
                case Account.AccountType.Student: return new Student(login, pass, reader);
            }
            throw new FormatException("Wrong account type");
        }

        public Account TryLogin(string login, string password)
        {
            using (BinaryReader reader = AccountsSettingsFile.GetFileReader())
            {
                while (reader.PeekChar() > -1)
                {
                    ushort len = reader.ReadUInt16();
                    long pos = reader.BaseStream.Position;
                    if (login == reader.ReadString())
                    {
                        if (password == reader.ReadString())
                            return LoadAccount(reader, login, password);
                    }
                    else
                        reader.BaseStream.Position = pos + len;
                }
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>string - login, int - position</returns>
        public Dictionary<string, int> GetAccountsList()
        {
            Dictionary<string, int> accounts = new Dictionary<string, int>();
            using (BinaryReader reader = AccountsSettingsFile.GetFileReader())
            {
                while (reader.PeekChar() > -1)
                {
                    int pos = (int)reader.BaseStream.Position;
                    ushort len = reader.ReadUInt16();
                    string login = reader.ReadString();

                    accounts.Add(login, pos);

                    reader.BaseStream.Position = pos + len + 2;
                }
                return accounts;
            }
        }
    }
}
/* Структура файлов:
 * Accounts.dat 
 * [
 *   length : ushort   --! размер данных аккаунта не должен превышать 65535 байт
 *   login : string,
 *   pass : string,
 *   тип учётной записи : (AccountType : byte),
 *   ... (параметры)
 * ]
 * 
 */