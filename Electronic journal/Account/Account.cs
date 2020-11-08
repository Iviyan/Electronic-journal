using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Electronic_journal
{
    public abstract class Account : ICustomSerializable
    {
        public string Login { get; }
        public string Password { get; protected set; }

        public enum AccountType : byte
        {
            Admin = 1,
            Teacher,
            Student
        }
        public AccountType Type { get; }

        public Account(string login, string password, AccountType type)
        {
            Login = login;
            Password = password;
            Type = type;
        }
        public Account(BinaryReader reader, AccountType type) : this(reader.ReadString(), reader.ReadString(), type) { reader.BaseStream.Position++; }

        public virtual void Export(BinaryWriter writer)
        {
            writer.Write(Login);
            writer.Write(Password);
            writer.Write((byte)Type);
        }
    }
}
