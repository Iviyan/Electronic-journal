using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Electronic_journal
{
    public class Teacher : Person, ICustomSerializable
    {
        

        public Teacher(string login, string password, string firstName, string lastName, string patronymic, DateTime birthday)
            : base(login, password, AccountType.Teacher, firstName, lastName, patronymic, birthday)
        {
        }

        public new void Export(BinaryWriter writer)
        {
            base.Export(writer);
            //writer.Write()

            writer.Flush();
        }

        public static new void SkipRead(BinaryReader reader)
        {
            Person.SkipRead(reader);
        }
    }
}
