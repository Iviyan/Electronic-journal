using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Electronic_journal.Account;

namespace Electronic_journal
{
    public class Settings
    {
        public string FolderName { get; }
        public string AccountsFolderName { get; }
        public string GetAccountFilePatch(string login) => @$"{AccountsFolderName}\{login}.dat";
        public string GroupsFolderName { get; }
        public string GetGroupFolderPatch(string groupName) => $@"{GroupsFolderName}\{groupName}";
        public string GetGroupInfoFilePatch(string groupName) => $@"{GetGroupFolderPatch(groupName)}\info.dat";
        public string GetGroupJournalFilePatch(string groupName) => $@"{GetGroupFolderPatch(groupName)}\journal.dat";
        public SettingsFile AccountsListFile { get; }
        public SettingsFile GroupsListFile { get; }
        public bool CheckAccountFile(string login) => File.Exists(@$"{AccountsFolderName}\{login}.dat");
        public BinaryReader GetAccountFileReader(string login, FileMode fileMode = FileMode.Open) => new BinaryReader(File.Open(@$"{AccountsFolderName}\{login}.dat", fileMode));
        public BinaryWriter GetAccountFileWriter(string login, FileMode fileMode = FileMode.Truncate) => new BinaryWriter(File.Open(@$"{AccountsFolderName}\{login}.dat", fileMode));

        public const string DefaultAdminLogin = "admin";
        public const string DefaultAdminPassword = "admin";

        private List<string> subjects = new List<string>();
        public string[] Subjects { get => subjects.ToArray(); }

        public Settings(
            string folderName = "Electronic Journal",
            string accountsFolderName = "Accounts",
            string groupsFolderName = "Groups",
            string accountsFileName = "Accounts.dat",
            string groupsFileName = "Groups.dat"
            )
        {
            FolderName = folderName;
            AccountsFolderName = $@"{folderName}\{accountsFolderName}";
            GroupsFolderName = $@"{folderName}\{groupsFolderName}";
            AccountsListFile = new SettingsFile(accountsFileName, folderName);
            GroupsListFile = new SettingsFile(groupsFileName, folderName);

            if (!Directory.Exists(FolderName)) Directory.CreateDirectory(FolderName);
            if (!Directory.Exists(AccountsFolderName)) Directory.CreateDirectory(AccountsFolderName);
            if (!Directory.Exists(GroupsFolderName)) Directory.CreateDirectory(GroupsFolderName);

            if (!AccountsListFile.Exists)
                AddAccount(new Admin(DefaultAdminLogin, DefaultAdminPassword));
            if (!GroupsListFile.Exists)
                File.Create(GroupsListFile.FilePatch);

        }

        public void AddAccount(Account account)
        {
            using (BinaryWriter writer = AccountsListFile.GetFileWriter())
                writer.Write(account.Login);
            using (BinaryWriter writer = new BinaryWriter(File.Open(GetAccountFilePatch(account.Login), FileMode.Create)))
                account.Export(writer);
        }
        public void RemoveAccount(string login)
        {
            List<string> accounts = new();
            using (BinaryReader reader = AccountsListFile.GetFileReader())
                while (reader.PeekChar() > -1)
                    accounts.Add(reader.ReadString());
            accounts.Remove(login);
            using (BinaryWriter writer = AccountsListFile.GetFileWriter(FileMode.Truncate))
                for (int i = 0; i < accounts.Count; i++)
                    writer.Write(accounts[i]);

            File.Delete(GetAccountFilePatch(login));
        }
        public void RenameAccount(string oldLogin, string newLogin)
        {
            List<string> accounts = new();
            using (BinaryReader reader = AccountsListFile.GetFileReader())
                while (reader.PeekChar() > -1)
                {
                    string login = reader.ReadString();
                    if (login == oldLogin) login = newLogin;
                    accounts.Add(login);
                }

            using (BinaryWriter writer = AccountsListFile.GetFileWriter(FileMode.Truncate))
                for (int i = 0; i < accounts.Count; i++)
                    writer.Write(accounts[i]);

            File.Move(GetAccountFilePatch(oldLogin), GetAccountFilePatch(newLogin));
        }

        public Account LoadAccount(string login)
        {
            if (CheckAccountFile(login))
                using (BinaryReader reader = GetAccountFileReader(login))
                    return LoadAccount(reader);
            else
                return null;
        }
        public Account LoadAccount(BinaryReader reader) => LoadAccount(reader, reader.ReadByte(), reader.ReadString(), reader.ReadString());
        public Account LoadAccount(BinaryReader reader, byte type_, string login, string pass)
        {
            AccountType type = (AccountType)type_;
            switch (type)
            {
                case AccountType.Admin: return new Admin(login, pass);
                case AccountType.Teacher: return new Teacher(login, pass, reader);
                case AccountType.Student: return new Student(login, pass, reader);
            }
            throw new FormatException("Wrong account type");
        }

        public Account TryLogin(string login, string password, out string errorMsg)
        {
            bool accountExists = false;
            using (BinaryReader reader = AccountsListFile.GetFileReader())
                while (reader.PeekChar() > -1)
                    if (reader.ReadString() == login)
                    {
                        accountExists = true;
                        break;
                    }
            if (!accountExists) {
                errorMsg = "Аккаунта не существует";
                return null;
            }

            using (BinaryReader reader = GetAccountFileReader(login))
            {
                byte type = reader.ReadByte();
                reader.SkipString();
                if (reader.ReadString() != password)
                {
                    errorMsg = "Неверный пароль";
                    return null;
                }
                errorMsg = String.Empty;
                return LoadAccount(reader, type, login, password);
            }

        }
        public string[] GetAccountsList()
        {
            List<string> accounts = new();
            using (BinaryReader reader = AccountsListFile.GetFileReader())
                while (reader.PeekChar() > -1)
                    accounts.Add(reader.ReadString());
            return accounts.ToArray();
        }
        public string[] GetGroupsList()
        {
            List<string> groups = new();
            using (BinaryReader reader = GroupsListFile.GetFileReader())
                while (reader.PeekChar() > -1)
                    groups.Add(reader.ReadString());
            return groups.ToArray();
        }

        public void AddGroup(Group group)
        {
            using (BinaryWriter writer = GroupsListFile.GetFileWriter())
                writer.Write(group.Name);
            group.Export(this);
        }

        public void RemoveGroup(string groupName)
        {
            List<string> groups = new();
            using (BinaryReader reader = GroupsListFile.GetFileReader())
                while (reader.PeekChar() > -1)
                    groups.Add(reader.ReadString());
            groups.Remove(groupName);

            using (BinaryWriter writer = GroupsListFile.GetFileWriter(FileMode.Truncate))
                for (int i = 0; i < groups.Count; i++)
                    writer.Write(groups[i]);

            Directory.Delete(GetGroupFolderPatch(groupName), true);
        }

    }
}
/* Структура файлов:
 * 
 * Accounts.dat = [login,]
 * Groups.dat = [GroupName,]
 * 
 * Accounts\<Account login>.dat
 * {
 *   тип учётной записи : (AccountType : byte),
 *   login : string,
 *   pass : string,
 *   ... (параметры)
 * }
 * 
 * Groups\<Group name>\info.dat
 * {
 *   Number of disciplines : byte,
 *   [ discipline : string, ],
 *   
 *   Number of teachers : byte,
 *   [
 *     Teacher's login : string,
 *     Number of disciplines : byte,
 *     [ disciplineID  : byte, ]
 *   ]
 *   
 *   Number of students : byte,
 *   [ Student's login : string, ]
 * }
 * 
 * Groups\<Group name>\journal.dat
 * [                      // index ~ id of student
 *   [                    // index ~ id of discipline
 *     number of marks : byte,
 *     [
 *       mark : byte,
 *       date : (DateTime : long)
 *     ]
 *   ]
 * ]
 * 
 */