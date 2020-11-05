using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Electronic_journal
{
    public abstract class Person : Account
    {
        public string FirstName { get; protected set; }
        public string LastName { get; protected set; }
        public string Patronymic { get; protected set; }
        public DateTime Birthday { get; protected set; }
        public Person(string login, string password, AccountType type, string firstName, string lastName, string patronymic, DateTime birthday) : base(login, password, type)
        {
            FirstName = firstName;
            LastName = lastName;
            Patronymic = patronymic;
            Birthday = birthday;
        }

        public override void Export(BinaryWriter writer)
        {
            base.Export(writer);
            writer.Write(FirstName);
            writer.Write(LastName);
            writer.Write(Patronymic);
            writer.Write(Birthday);
        }

        public static void SkipRead(BinaryReader reader)
        {
            reader.SkipString(); //
            reader.SkipString();
            reader.SkipString();
            reader.ReadInt64(); // DateTime
        }
    }
}
