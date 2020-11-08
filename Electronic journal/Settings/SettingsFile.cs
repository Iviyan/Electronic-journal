using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Electronic_journal
{
    public class SettingsFile
    {
        public string FolderName { get; }
        public string FileName { get; }

        public readonly string FilePatch;

        public bool Exists => File.Exists(FilePatch);

        public FileStream GetsFileStream(FileMode fileMode = FileMode.OpenOrCreate) => File.Open(FilePatch, fileMode);
        public FileStream GetFileStream(FileMode fileMode = FileMode.OpenOrCreate) => File.Open(FilePatch, fileMode);

        public BinaryWriter GetFileWriter(FileMode fileMode = FileMode.Append) => new BinaryWriter(GetFileStream(fileMode));
        public BinaryReader GetFileReader(FileMode fileMode = FileMode.Open) => new BinaryReader(GetFileStream(fileMode));

        public SettingsFile(
            string fileName,
            string folderName = ""
            )
        {
            FileName = fileName;
            FolderName = folderName;

            FilePatch = String.IsNullOrEmpty(FolderName) ? FileName : @$"{FolderName}\{FileName}";
        }
    }
}