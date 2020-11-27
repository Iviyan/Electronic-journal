using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electronic_journal
{
    public class JournalViewer
    {
        public Group Group_;
        public Student Student_;

        public JournalViewer(Group group, Student student)
        {
            Group_ = group;
            Student_ = student;
        }

        public void View()
        {
            string[] choices = Group_.Disciplines.Append("Назад").ToArray();
            ConsoleSelect menu = new(choices);

            int studentIndex = Array.IndexOf(Group_.Students.ToArray(), Student_.Login);
            void WriteMarks()
            {
                int marksStartX = menu.CurrentMaxWidth + 4;
                for (int i = 0; i < Group_.Marks[studentIndex].Count; i++)
                {
                    Console.SetCursorPosition(marksStartX, i);
                    string smarks = Group_.Marks[studentIndex][i].Aggregate("", (acc, mark) => acc += $"{mark.mark} ");
                    if (Console.WindowWidth - 1 - marksStartX - smarks.Length < 0)
                        smarks = smarks.Substring(0, Console.WindowWidth - 1 - marksStartX);
                    Console.Write(smarks);
                }
            }
            WriteMarks();
            void ClearThis()
            {
                ClearMarks();
                ConsoleHelper.ClearArea(0, 0, Console.WindowWidth - 1, 1 + choices.Length);
                Console.SetCursorPosition(0, 0);
            }


            int select = 0,
                lastWritedMarks = -1;

            void ClearMarks()
            {
                if (lastWritedMarks >= 0)
                    ConsoleHelper.ClearArea(0, menu.CurrentHeight + 2, 14, menu.CurrentHeight + 2 + Group_.Marks[studentIndex][lastWritedMarks].Count);
            }

            while (true)
            {
                select = menu.Choice(select,
                    (ConsoleKeyInfo key, int selectedIndex) =>
                    {
                        if (key.Key == ConsoleKey.Escape) return -1;
                        return null;
                    });
                if (select == -1) break;
                if (select == choices.Length - 1) break;

                ClearMarks();

                var marks = Group_.Marks[studentIndex][select];
                using (new CursonPosition(0, menu.CurrentHeight + 2))
                    for (int i = 0; i < marks.Count; i++)
                    {
                        Console.WriteLine($"{marks[i].date.ToString("d")} - {marks[i].mark}");
                    }
                lastWritedMarks = select;
            }
            ClearThis();
        }
    }
}
