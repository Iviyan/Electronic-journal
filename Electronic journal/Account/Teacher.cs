using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Electronic_journal
{
    public class Teacher : Person, ICustomSerializable, IUI
    {
        public string[] Subjects { get; set; }
        public string[] Groups { get; set; }

        public Teacher(string login, string password, string firstName, string lastName, string patronymic, DateTime birthday, string[] subjects, string[] groups)
            : base(login, password, AccountType.Teacher, firstName, lastName, patronymic, birthday)
        {
            Subjects = subjects;
            Groups = groups;
        }
        public Teacher(string login, string password, BinaryReader reader)
            : base(login, password, AccountType.Teacher, reader)
        {
            Subjects = reader.ReadStringArray();
            Groups = reader.ReadStringArray();
        }
        public Teacher(BinaryReader reader)
            : base(AccountType.Teacher, reader)
        {
            Subjects = reader.ReadStringArray();
            Groups = reader.ReadStringArray();
        }

        public new void Export(BinaryWriter writer)
        {
            base.Export(writer);
            writer.Write(Subjects);
            writer.Write(Groups);
        }

        public void UI()
        {

        }
    }
}
