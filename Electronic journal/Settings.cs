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
        public string JournalDataFileName { get; }

        public readonly string AccountsDataFilePatch;
        public readonly string JournalDataFilePatch;

        public readonly BinaryWriter AccountsDataFileWriter;
        public readonly BinaryWriter JournalDataFileWriter;

        public const string DefaultAdminLogin = "admin";
        public const string DefaultAdminPassword = "admin";

        public Settings(
            string folderName = "Electronic Journal",
            string accountsDataFileName = "Accounts.dat",
            string journalDataFileName = "Journal.dat"
            )
        {
            FolderName = folderName;
            AccountsDataFileName = accountsDataFileName;
            JournalDataFileName = journalDataFileName;

            AccountsDataFilePatch = @$"{FolderName}\{AccountsDataFileName}";
            JournalDataFilePatch = @$"{FolderName}\{JournalDataFilePatch}";

            if (!Directory.Exists(FolderName)) Directory.CreateDirectory(FolderName);

            FileStream AccountsDataFileStream;
            bool IsAccountsDataFileCreated = !File.Exists(AccountsDataFilePatch);
            AccountsDataFileStream = File.Open(AccountsDataFilePatch, FileMode.OpenOrCreate);
            AccountsDataFileWriter = new BinaryWriter(AccountsDataFileStream);

            if (IsAccountsDataFileCreated)
                new Admin(DefaultAdminLogin, DefaultAdminPassword).Export(AccountsDataFileWriter);
            
            FileStream JournalsDataFileStream;
            bool IsJournalDataFileCreated = !File.Exists(AccountsDataFilePatch);
            AccountsDataFileStream = File.Open(AccountsDataFilePatch, FileMode.OpenOrCreate);
            AccountsDataFileWriter = new BinaryWriter(AccountsDataFileStream);

            if (IsAccountsDataFileCreated)
                new Admin(DefaultAdminLogin, DefaultAdminPassword).Export(AccountsDataFileWriter);
        }
    }
}