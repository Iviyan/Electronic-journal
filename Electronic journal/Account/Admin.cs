using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Electronic_journal
{
    public class Admin : Account, ICustomSerializable
    {
        public Admin(string login, string password) : base(login, password, AccountType.Admin) { }
        public Admin(BinaryReader reader) : base(reader, AccountType.Admin) { }

        public override void Export(BinaryWriter writer)
        {
            base.Export(writer);
            writer.Flush();
        }
    }
}