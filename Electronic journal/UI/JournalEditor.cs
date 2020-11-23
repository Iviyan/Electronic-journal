using static Electronic_journal.Account;

namespace Electronic_journal
{
    public class JournalEditor
    {
        public Group Group_;
        public Account Account_;
        public Settings Settings_;

        byte[] Disciplines;

        public JournalEditor(Group group, Account account, Settings settings)
        {
            Group_ = group;
            Account_ = account;
            Settings_ = settings;

            switch (Account_.Type)
            {
                case AccountType.Admin:
                    {
                        Disciplines = new byte[Group_.Disciplines.Count];
                        for (byte i = 0; i < Group_.Disciplines.Count; i++)
                            Disciplines[i] = i;
                        break;
                    }

                case AccountType.Teacher:
                    {
                        int teacherIndex = group.Teachers.IndexOf(account.Login);
                        if (teacherIndex >= 0) {
                           // Disciplines = new byte[Group_.Teacher_disciplines[];
                            //for (byte i = 0; i < Group_.Disciplines.Count; i++)
                            //    Disciplines.Add(i, Group_.Disciplines[i]);
                        } else
                        {

                        }
                        break;
                    }
            }
        }

        public bool Edit()
        {
            
           // ConsoleSelect disciplinesMenu = new();

            return true;
        }

        public void Clear()
        {

        }
    }
}
