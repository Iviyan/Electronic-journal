using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Electronic_journal
{
    public class TeachersAndDisciplinesEditor : ICustomEditor<Group>
    {
        public string PropertyName { get; set; }
        public Group Obj { get; set; }
        public int StartY;

        int ChoicesCount;

        int LastError = -1;

        public TeachersAndDisciplinesEditor(Group group, string propertyName)
        {
            Obj = group;
            PropertyName = propertyName;
        }

        public string GetStringValue() => "...";

        static Regex numberArray = new Regex(@"^(\d+ *)*$", RegexOptions.Compiled);
        public bool Edit(int startY)
        {
            StartY = startY;
            var elements = new (string name, string value)[Obj.Teachers.Count];
            int ind = 0;
            foreach (var kv in Obj.Teacher_disciplines)
                elements[ind++] = (Obj.Teachers[kv.Key], String.Join(' ', kv.Value));

            ChoicesCount = elements.Length + 1;

            KeyValueEditor editor = new(
                elements,
                (int changedIndex, string value, KeyValueEditor editor) =>
                {
                    if (!numberArray.IsMatch(value))
                    {
                        editor.SelectColor(changedIndex, ConsoleColor.Red);
                        WriteError("Ввод должен состоять из чисел, разделяемых пробелом");
                        LastError = changedIndex;
                        return null;
                    }
                    else
                    {
                        var Disciplines = Obj.Disciplines;

                        string[] strBytes = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        SortedSet<byte> disciplines = new();
                        for (int i = 0; i < strBytes.Length; i++)
                        {
                            if (!byte.TryParse(strBytes[i], out byte disciplineIndex))
                            {
                                editor.SelectColor(changedIndex, ConsoleColor.Red);
                                WriteError("Все индексы должны быть меньше 255");
                                LastError = changedIndex;
                                return null;
                            }
                            if (disciplineIndex >= Disciplines.Count)
                            {
                                editor.SelectColor(changedIndex, ConsoleColor.Red);
                                WriteError($"Дисциплины с номером {disciplineIndex} не существует");
                                LastError = changedIndex;
                                return null;
                            }
                            disciplines.Add(disciplineIndex);
                        }
                        Obj.Teacher_disciplines[(byte)changedIndex] = new List<byte>(disciplines);

                        if (LastError >= 0)
                        {
                            ClearDisciplines();
                            ClearError();
                            LastError = -1;
                        }

                        WriteDiscipliness(disciplines.ToArray());
                        return String.Join(' ', disciplines);
                    }
                },
                (int selectedIndex, KeyValueEditor editor) =>
                {
                    bool error = LastError >= 0;
                    if (error)
                    {
                        editor.UpdateValue(LastError, String.Join(' ', Obj.Teacher_disciplines[(byte)LastError]));
                        ClearDisciplines();
                        ClearError();
                        LastError = -1;
                    }
                    if (selectedIndex >= 0)
                        WriteDiscipliness(Obj.Teacher_disciplines[(byte)selectedIndex].ToArray());
                    else
                        if (!error)
                            ClearDisciplines();
                },
                StartY
            );

            WriteDiscipliness(elements.Length > 0 ? Obj.Teacher_disciplines[0].ToArray() : null);

            editor.Edit(out var elem);

            return true;
        }

        int ErrorHeight = 0;
        void WriteError(string msg)
        {
            int errorHeight = (int)Math.Ceiling($"!> {msg}".Length / (double)Console.WindowWidth);
            bool errorHeightChanged = errorHeight != ErrorHeight;

            if (ErrorHeight > 0) ClearError();
            if (errorHeightChanged)
            {
                ClearDisciplines();
                ErrorHeight = errorHeight;
                WriteDiscipliness();
            }
            using (new CursonPosition(0, StartY + ChoicesCount + 1))
                Console.Write($"!> {msg}");

            if (!errorHeightChanged) ErrorHeight = errorHeight;
        }
        void ClearError()
        {
            ConsoleHelper.ClearArea(0, StartY + Obj.Teachers.Count + 1, Console.WindowWidth - 1, StartY + ChoicesCount + ErrorHeight);
            ErrorHeight = 0;
        }

        int ErrorStartY => StartY + ChoicesCount + (ErrorHeight > 0 ? ErrorHeight + 1 : 0) + 1;
        void WriteDiscipliness(byte[] highlight = null)
        {
            var disciplines = Obj.Disciplines;
            Console.SetCursorPosition(0, ErrorStartY);
            if (highlight == null)
            {
                for (byte i = 0; i < disciplines.Count; i++)
                    Console.WriteLine($"{i}. {disciplines[i]}");
            }
            else
            {
                for (byte i = 0; i < disciplines.Count; i++)
                    if (highlight.Contains(i))
                        using (new UseConsoleColor(ConsoleColor.Green))
                            Console.WriteLine($"{i}. {disciplines[i]}");
                    else
                        Console.WriteLine($"{i}. {disciplines[i]}");
            }
        }
        void ClearDisciplines()
        {
            int startY = ErrorStartY,
                endY = startY + Obj.Disciplines.Count - 1;
            ConsoleHelper.ClearArea(0, startY, Console.WindowWidth - 1, endY);
        }

        public void Clear()
        {
            ConsoleHelper.ClearArea(0, StartY, Console.WindowWidth - 1, StartY + Obj.Teachers.Count);
        }
    }
}
