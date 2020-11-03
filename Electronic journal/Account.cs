using System;
using System.Collections.Generic;
using System.Text;

namespace Electronic_journal
{
    public abstract class Account
    {
        public string Login { get; protected set; }
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
    }
}
