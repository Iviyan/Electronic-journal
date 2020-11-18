using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Electronic_journal
{
    public class Student : Person, ICustomSerializable, IUI
    {
        [Editor("Группа"), StringParams(AllowEmpty = false)]
        public string Group { get; set; }

        public Student(string login, string password, string firstName, string lastName, string patronymic, DateTime birthday, string group)
            : base(login, password, AccountType.Student, firstName, lastName, patronymic, birthday)
        {
            Group = group;
        }
        public Student(string login, string password, BinaryReader reader)
            : base(login, password, AccountType.Student, reader)
        {
            Group = reader.ReadString();
        }
        public Student(BinaryReader reader)
            : base(AccountType.Student, reader)
        {
            Group = reader.ReadString();
        }

        public override void Export(BinaryWriter writer)
        {
            base.Export(writer);
            writer.Write(Group);
        }

        public void UI(Settings settings)
        {

        }
    }
}
