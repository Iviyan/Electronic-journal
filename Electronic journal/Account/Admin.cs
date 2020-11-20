
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Electronic_journal
{
    public class Admin : Account, ICustomSerializable, IUI
    {
        public Admin(string login, string password) : base(login, password, AccountType.Admin) { }
        public Admin(BinaryReader reader) : base(reader, AccountType.Admin) { }

        public override void Export(BinaryWriter writer)
        {
            base.Export(writer);
        }
        public void UI(Settings settings)
        {
            new AdminUI(this, settings);
        }
    }
}