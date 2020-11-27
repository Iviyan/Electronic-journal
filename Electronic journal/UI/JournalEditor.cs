using System;
using System.Linq;
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
                        if (teacherIndex >= 0)
                        {
                            Disciplines = Group_.Teacher_disciplines[(byte)teacherIndex].ToArray();
                        }
                        break;
                    }
            }

            if (Disciplines == null) Disciplines = new byte[] { };
        }

        public bool Edit()
        {
            string[] choices = Disciplines.Select(b => Group_.Disciplines[b]).Append("Сохранить").ToArray();
            ConsoleSelect menu = new(choices, write: false);

            int select = 0;
            while (true)
            {
                select = menu.Choice(select,
                    (ConsoleKeyInfo key, int selectedIndex) =>
                    {
                        if (key.Key == ConsoleKey.Escape) return -1;
                        return null;
                    }, true);
                if (select == -1) break;
                if (select == choices.Length - 1) break;
                menu.Clear();
                EditDiscipline(select);
            }
            menu.Clear();
            return select >= 0;
        }
        bool EditDiscipline(int disciplineIndex)
        {
            string[] choices = Group_.Students
                .Select(
                    studentLogin => {
                        Student st = Settings_.LoadAccount(studentLogin) as Student;
                        return UIFunctions.GetInitials(st);
                    }
                )
                .Append("Сохранить")
                .ToArray();
            ConsoleSelect menu = new(choices, startY: 2, write: false);

            void WriteMarks()
            {
                int marksStartX = menu.CurrentMaxWidth + 4;
                for (int i = 0; i < Group_.Marks.Count; i++)
                {
                    Console.SetCursorPosition(marksStartX, 2 + i);
                    string smarks = Group_.Marks[i][disciplineIndex].Aggregate("", (acc, mark) => acc += $"{mark.mark} ");
                    if (Console.WindowWidth - 1 - marksStartX - smarks.Length < 0)
                        smarks = smarks.Substring(0, Console.WindowWidth - 1 - marksStartX);
                    Console.Write(smarks);
                }
            }
            WriteMarks();

            void ClearThis()
            {
                ConsoleHelper.ClearArea(0, 0, Console.WindowWidth - 1, 1 + choices.Length);
                Console.SetCursorPosition(0, 0);
            }

            int select = 0;
            while (true)
            {
                Console.SetCursorPosition(0, 0);
                ConsoleHelper.WriteCenter(Group_.Disciplines[disciplineIndex]);
                select = menu.Choice(select,
                    (ConsoleKeyInfo key, int selectedIndex) =>
                    {
                        if (key.Key == ConsoleKey.Escape) return -1;
                        return null;
                    }, true);
                if (select == -1) { ClearThis(); return false; }
                if (select == choices.Length - 1) break;
                ClearThis();
                EditStudentMarks(disciplineIndex, select);
                WriteMarks();
            }

            ClearThis();
            return true;
        }

        class DateMark
        {
            [Editor("Дата"), DateTimeParams(OnlyDate = true)]
            public DateTime Date { get; set; }
            [Editor("Оценка")]
            public byte Mark { get; set; }
            public DateMark(byte mark, DateTime date) =>
                (Mark, Date) = (mark, date);
            public DateMark((byte mark, DateTime date) m) =>
                (Mark, Date) = (m.mark, m.date);
            public static explicit operator (byte mark, DateTime date)(DateMark dmark) => (dmark.Mark, dmark.Date);
            
        }
        static bool validateMark(DateMark mark, string[] changedProperties, out string msg)
        {
            if (changedProperties.Contains(nameof(mark.Mark)) && (mark.Mark < 0 || mark.Mark > 5))
            {
                msg = "Оценка должна быть в диапазоне 0..5";
                return false;

            }
            msg = ""; return true;
        }
        bool EditStudentMarks(int disciplineIndex, int studentIndex)
        {
            ConsoleHelper.WriteCenter($"{Group_.Disciplines[disciplineIndex]} - {UIFunctions.GetFullName(Settings_.LoadAccount(Group_.Students[studentIndex]) as Student)}");
            string[] choices = Group_.Marks[studentIndex][disciplineIndex]
                .Select(
                    mark => $"{mark.date.ToString("d")} - {mark.mark}"
                )
                .Append("Добавить")
                .Append("Сохранить")
                .ToArray();
            ConsoleSelect menu = new(choices, startY: 2, maxHeight:Console.WindowHeight - 1 - 2 - 5);

            int select = 0;
            while (true)
            {
                select = menu.Choice(select,
                    (ConsoleKeyInfo key, int selectedIndex) =>
                    {
                        if (key.Key == ConsoleKey.Escape) return -1;
                        if (key.Key == ConsoleKey.Delete)
                        {
                            Group_.Marks[studentIndex][disciplineIndex].RemoveAt(selectedIndex);
                            menu.Choices.RemoveAt(selectedIndex);
                        }
                        return null;
                    });
                if (select == -1) break;
                if (select == menu.Choices.Count - 1) break; // Сохранить
                if (select == menu.Choices.Count - 2) // Добавить
                {
                    int startY = 2 + menu.CurrentHeight + 1;
                    DateMark mark = new(5, DateTime.Today);
                    ClassEditor<DateMark> editor = new(mark, validateMark, startY);
                    if (editor.Edit())
                    {
                        menu.Choices.Insert(Group_.Marks[studentIndex][disciplineIndex].Count, $"{mark.Date.ToString("d")} - {mark.Mark}");
                        Group_.Marks[studentIndex][disciplineIndex].Add(((byte, DateTime))mark);
                    }
                    editor.Clear();
                    continue;
                }
                {
                    int startY = 2 + menu.CurrentHeight + 1;
                    var mark_ = Group_.Marks[studentIndex][disciplineIndex][select];
                    DateMark mark = new(mark_);
                    ClassEditor<DateMark> editor = new(mark, validateMark, startY);
                    if (editor.Edit())
                    {
                        Group_.Marks[studentIndex][disciplineIndex][select] = ((byte, DateTime))mark;
                        menu.Choices[select] = $"{mark.Date.ToString("d")} - {mark.Mark}";
                    }
                    editor.Clear();
                }
            }
            ConsoleHelper.ClearArea(0, 0, Console.WindowWidth - 1, 0);
            menu.Clear();
            return select >= 0;
        }
    }
}
