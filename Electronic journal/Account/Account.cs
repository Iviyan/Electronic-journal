using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Electronic_journal
{
    public abstract class Account : ICustomSerializable
    {
        [Editor("Логин"), StringParams(AllowEmpty = false)]
        public string Login { get; set; }
        [Editor("Пароль"), StringParams(AllowEmpty = false)]
        public string Password { get; set; }

        public enum AccountType : byte
        {
            Admin = 1,
            Teacher = 2,
            Student = 3
        }
        public AccountType Type { get; }

        public Account(string login, string password, AccountType type)
        {
            Login = login;
            Password = password;
            Type = type;
        }
        public Account(BinaryReader reader, AccountType type)
        {
            Login = reader.ReadString();
            Password = reader.ReadString();
        }

        public virtual void Export(BinaryWriter writer)
        {
            writer.Write((byte)Type);
            writer.Write(Login);
            writer.Write(Password);
        }
    }
}
