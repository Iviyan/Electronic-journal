using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Electronic_journal
{
    public class Admin : Account, ICustomSerializable
    {
        public Admin(string login, string password) : base(login, password, AccountType.Admin)
        {
        }

        public void Export(BinaryWriter writer)
        {
            writer.Write((byte)AccountType.Admin);
            writer.Write(Login);
            writer.Write(Password);
            writer.Flush();
        }
    }
}
