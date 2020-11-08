using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Electronic_journal
{
    public class Student : Person, ICustomSerializable
    {
        public string Group { get; private set; }

        public Student(string login, string password, string firstName, string lastName, string patronymic, DateTime birthday, string group)
            : base(login, password, AccountType.Teacher, firstName, lastName, patronymic, birthday)
        {
            Group = group;
        }
        public Student(string login, string password, BinaryReader reader)
            : base(login, password, AccountType.Teacher, reader)
        {
            Group = reader.ReadString();
        }
        public Student(BinaryReader reader)
            : base(AccountType.Teacher, reader)
        {
            Group = reader.ReadString();
        }

        public new void Export(BinaryWriter writer)
        {
            base.Export(writer);
            writer.Write(Group);
        }

    }
}
