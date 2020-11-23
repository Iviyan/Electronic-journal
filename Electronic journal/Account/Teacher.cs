using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Electronic_journal
{
    public class Teacher : Person, ICustomSerializable, IUI
    {
        [Editor("Дисциплины", ReadOnly = true)]
        public string[] Disciplines { get; set; }
        [Editor("Группы", ReadOnly = true)]
        public string[] Groups { get; set; }

        public Teacher(string login, string password, string firstName, string lastName, string patronymic, DateTime birthday, string[] subjects, string[] groups)
            : base(login, password, AccountType.Teacher, firstName, lastName, patronymic, birthday)
        {
            Disciplines = subjects;
            Groups = groups;
        }
        public Teacher(string login, string password, BinaryReader reader)
            : base(login, password, AccountType.Teacher, reader)
        {
            Disciplines = reader.ReadStringArray();
            Groups = reader.ReadStringArray();
        }
        public Teacher(BinaryReader reader)
            : base(AccountType.Teacher, reader)
        {
            Disciplines = reader.ReadStringArray();
            Groups = reader.ReadStringArray();
        }

        public override void Export(BinaryWriter writer)
        {
            base.Export(writer);
            writer.Write(Disciplines);
            writer.Write(Groups);
        }

        public void UI(Settings settings)
        {

        }
    }
}
