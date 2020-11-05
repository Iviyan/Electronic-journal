using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Electronic_journal
{
    public class Settings
    {
        public string FolderName { get; }
        public string AccountsDataFileName { get; }
        public string SubjectsDataFileName { get; }
        public string GroupsDataFileName { get; }

        public readonly string AccountsDataFilePatch;
        public readonly string SubjectsDataFilePatch;
        public readonly string GroupsDataFilePatch;

        FileStream GetAccountsDataFileStream(FileMode fileMode = FileMode.OpenOrCreate) => File.Open(AccountsDataFilePatch, fileMode);
        FileStream GetSubjectsDataFileStream(FileMode fileMode = FileMode.OpenOrCreate) => File.Open(SubjectsDataFilePatch, fileMode);
        FileStream GetGroupsDataFileStream(FileMode fileMode = FileMode.OpenOrCreate) => File.Open(SubjectsDataFilePatch, fileMode);

        public BinaryWriter GetAccountsDataFileWriter(FileMode fileMode = FileMode.Append) => new BinaryWriter(GetAccountsDataFileStream(fileMode));
        public BinaryReader GetAccountsDataFileReader(FileMode fileMode = FileMode.Open) => new BinaryReader(GetAccountsDataFileStream(fileMode));
        public BinaryWriter GetSubjectsDataFileWriter(FileMode fileMode = FileMode.Append) => new BinaryWriter(GetSubjectsDataFileStream(fileMode));
        public BinaryReader GetSubjectsDataFileReader(FileMode fileMode = FileMode.Open) => new BinaryReader(GetSubjectsDataFileStream(fileMode));
        public BinaryWriter GetGroupsDataFileWriter(FileMode fileMode = FileMode.Append) => new BinaryWriter(GetGroupsDataFileStream(fileMode));
        public BinaryReader GetGroupsDataFileReader(FileMode fileMode = FileMode.Open) => new BinaryReader(GetGroupsDataFileStream(fileMode));

        public const string DefaultAdminLogin = "admin";
        public const string DefaultAdminPassword = "admin";

        private List<string> subjects = new List<string>();
        public string[] Subjects { get => subjects.ToArray(); }

        public Settings(
            string folderName = "Electronic Journal",
            string accountsDataFileName = "Accounts.dat",
            string subjectsDataFileName = "Subjects.dat",
            string groupsDataFileName = "Groups.dat"
            )
        {
            FolderName = folderName;
            AccountsDataFileName = accountsDataFileName;
            SubjectsDataFileName = subjectsDataFileName;
            GroupsDataFileName = groupsDataFileName;

            AccountsDataFilePatch = @$"{FolderName}\{AccountsDataFileName}";
            SubjectsDataFilePatch = @$"{FolderName}\{SubjectsDataFileName}";

            if (!Directory.Exists(FolderName)) Directory.CreateDirectory(FolderName);

            if (!File.Exists(AccountsDataFilePatch))
                using (BinaryWriter writer = GetAccountsDataFileWriter())
                    new Admin(DefaultAdminLogin, DefaultAdminPassword).Export(writer);

            if (!File.Exists(SubjectsDataFilePatch))
                GetSubjectsDataFileStream().Dispose();
            else
            {
                using (BinaryReader reader = GetSubjectsDataFileReader())
                    while (reader.PeekChar() > -1)
                        subjects.Add(reader.ReadString());
            }

        }

        public bool TryLogin(string login, string password)
        {
            using (BinaryReader reader = GetAccountsDataFileReader())
            {
                while (reader.PeekChar() > -1)
                {

                }
                return true;
            }
        }
    }
}
/* Структура файлов:
 * Accounts.dat 
 * [
 *   тип учётной записи : (AccountType : byte),
 *   login : string,
 *   pass : string,
 *   ... (параметры)
 * ]
 * 
 * Subjects.dat - string[] - список дисциплин
 * 
 */