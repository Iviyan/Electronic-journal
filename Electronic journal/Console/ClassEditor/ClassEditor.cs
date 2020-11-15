using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Electronic_journal
{
    public class ClassEditor<T>
    {
        public readonly T Obj;
        public (PropertyInfo property, string name, object initialValue)[] Properties;
        ValidateFunc Validate;

        public int StartY;
        int selectedIndex = 0;
        int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                Select(selectedIndex, ' ');
                selectedIndex = value;
                Select(selectedIndex);
                SetCursorPosition();
            }
        }

        int[] ValuesLength;

        public delegate bool ValidateFunc(T obj, out string msg);
        public ClassEditor(T obj, ValidateFunc validate)
        {
            Obj = obj;
            Validate = validate;

            var properties = new List<(PropertyInfo, string, object)>();
            int ind = 0;
            Type lastType = null;
            foreach (var property in typeof(T).GetProperties())
            {
                if (property.DeclaringType != lastType) // Иначе в первую очередь идут свойства производного класса, а потом уже базовых
                {
                    ind = 0;
                    lastType = property.DeclaringType;
                }

                var attribute = property.GetCustomAttributes<EditorAttribute>(true)
                                        .SingleOrDefault();
                if (attribute == null)
                    continue;

                properties.Insert(ind++, (property, attribute.DisplayValue, property.GetValue(obj)));
            }
            Properties = properties.ToArray();
            ValuesLength = new int[properties.Count];
        }

        string GetStringValueFromDateTime(PropertyInfo dateTimeProperty)
        {
            DateTime value = (DateTime)dateTimeProperty.GetValue(Obj);
            var dateTimeAttribute = dateTimeProperty.GetCustomAttributes<DateTimeParamsAttribute>(true)
                                    .SingleOrDefault();
            if (dateTimeAttribute != null)
                return GetStringValueFromDateTime(value, dateTimeAttribute);

            return value.ToString();
        }
        string GetStringValueFromDateTime(DateTime dateTime, DateTimeParamsAttribute attribute)
        {
            if (attribute.OnlyDate)
                return dateTime.ToString("d");

            return dateTime.ToString();
        }
        string GetStringValueFromStringArray(string[] stringArray) => Helper.ArrayToStr(stringArray);
        string GetStringValue(PropertyInfo property)
        {
            var value = property.GetValue(Obj);

            switch (value)
            {
                case DateTime: return GetStringValueFromDateTime(property);
                case string[] val: return GetStringValueFromStringArray(val);
            }

            return value.ToString();
        }

        void WriteValue(string val, int maxLength)
        {
            if (val.Length > maxLength)
            {
                Console.Write(val.Substring(0, maxLength - 3));
                using (new UseConsoleColor(ConsoleColor.Red))
                    Console.Write("...");
            }
            else
                Console.Write(val);

        }

        void Write()
        {
            for (int i = 0; i < Properties.Length; i++)
            {
                var p = Properties[i];
                Console.Write($"  {p.name}: ");
                int valueMaxLength = Console.WindowWidth - (p.name.Length + 4);
                string value = GetStringValue(p.property);
                ValuesLength[i] = value.Length;
                WriteValue(value, valueMaxLength);
                Console.CursorLeft = 0;
                Console.CursorTop++;
            }
            Console.WriteLine("  Сохранить");
        }

        void Select(int index, char c = '>')
        {
            Console.SetCursorPosition(0, StartY + index);
            Console.Write(c);
        }
        void SetCursorPosition()
        {
            if (SelectedIndex < Properties.Length) {
                int pos = Properties[SelectedIndex].name.Length + 4 + GetStringValue(Properties[SelectedIndex].property).Length;
                if (pos < Console.WindowWidth)
                    Console.CursorLeft = pos;
                else
                    Console.CursorLeft = Console.WindowWidth - 1;
            }
            else
                Console.CursorLeft = 0;
        }

        int errorMsgHeight = 0;
        void ClearError() => ConsoleHelper.ClearArea(1, StartY + Properties.Length + 1, Console.WindowWidth - 1, StartY + Properties.Length + 1 + errorMsgHeight);
        void writeError(string msg)
        {
            ClearError();
            using (new CursonPosition(1, StartY + Properties.Length + 1))
                //using (new UseConsoleColor(ConsoleColor.Red))
                Console.Write($"!> {msg}");
            errorMsgHeight = (int)Math.Ceiling($"!> {msg}".Length / (double)Console.WindowWidth);
        }

        void EditProperty(bool backspace = false)
        {
            var p = Properties[selectedIndex];

            void setCursorBeforeInput() => Console.CursorLeft = 2 + Properties[selectedIndex].name.Length + 2;
            setCursorBeforeInput();

            var value = p.property.GetValue(Obj);

            void updateCurrentValue(string value)
            {
                setCursorBeforeInput();
                int valueMaxLength = Console.WindowWidth - (p.name.Length + 4);
                WriteValue(value, valueMaxLength);
                if (value.Length < ValuesLength[SelectedIndex] && value.Length <= valueMaxLength)
                    if (ValuesLength[SelectedIndex] < valueMaxLength)
                        Console.Write(new string(' ', ValuesLength[SelectedIndex] - value.Length));
                    else
                        Console.Write(new string(' ', valueMaxLength - value.Length));

                ValuesLength[SelectedIndex] = value.Length;
            }

            if (backspace)
            {
                updateCurrentValue("");
                setCursorBeforeInput();
            }

            string input = "";
            switch (value)
            {
                case int val:
                    {

                        string sval = val.ToString();
                        if (Reader.ReadLine_esc(ref input, backspace ? "" : sval, false))
                        {
                            bool parse = int.TryParse(input, out int result);
                            updateCurrentValue(parse ? result.ToString() : sval);
                            if (parse)
                                p.property.SetValue(Obj, result);
                            else
                                writeError("Ошибка ввода числа");
                        }
                    }
                    break;
                case string val:
                    if (Reader.ReadLine_esc(ref input, backspace ? "" : val, false))
                        p.property.SetValue(Obj, input);
                    break;
                case DateTime val:
                    {
                        var attribute = p.property.GetCustomAttributes<DateTimeParamsAttribute>(true)
                                .SingleOrDefault();
                        string sval = GetStringValueFromDateTime(val, attribute);

                        if (Reader.ReadLine_esc(ref input, backspace ? "" : sval, false))
                        {
                            bool parse = DateTime.TryParse(input, out DateTime result);
                            updateCurrentValue(parse ? GetStringValueFromDateTime(result, attribute) : sval);
                            if (parse)
                                p.property.SetValue(Obj, result);
                            else
                                writeError("Ошибка ввода даты");
                        }
                    }
                    break;
                case string[] val:
                    {
                        
                        ClearError();

                        StringArrayEditor stringArrayEditor = new(val, $"{p.name}:", StartY + Properties.Length + 2);
                        bool esc = stringArrayEditor.Edit(true);
                        Console.CursorTop = StartY + SelectedIndex;
                        if (esc) {
                            p.property.SetValue(Obj, stringArrayEditor.GetArray());
                            updateCurrentValue(GetStringValueFromStringArray(stringArrayEditor.GetArray()));
                        }
                        stringArrayEditor.Clear();
                    }
                    break;
            }
        }

        bool CheckFields(out string msg)
        {
            for (int i = 0; i < Properties.Length; i++)
            {
                var value = Properties[i].property.GetValue(Obj);
                switch (value)
                {
                    case string val:
                        var attribute = Properties[i].property.GetCustomAttributes<StringParamsAttribute>(true)
                                .SingleOrDefault();
                        if (attribute != null && attribute.AllowEmpty == false && val == "")
                        {
                            msg = $"Поле {i} - \"{Properties[i].name}\" не может быть пустым";
                            return false;
                        }
                        break;
                }
            }
            msg = "";
            return true;
        }
        bool Finish()
        {
            string msg;
            bool success = CheckFields(out msg) && Validate(Obj, out msg);
            if (success)
            {
                ClearError();
                Console.CursorTop = StartY + Properties.Length + 2;
                return true;
            }
            else
            {
                writeError(msg);
                return false;
            }
        }

        public void Edit()
        {
            int count = Properties.Length + 1;

            Write();

            ConsoleKeyInfo info;

            Select(0);
            SetCursorPosition();

            while (true)
            {
                info = Console.ReadKey(true);

                switch (info.Key)
                {
                    case ConsoleKey.DownArrow:
                        if (SelectedIndex + 1 < count)
                            SelectedIndex++;
                        else
                            SelectedIndex = 0;
                        break;

                    case ConsoleKey.UpArrow:
                        if (SelectedIndex > 0)
                            SelectedIndex--;
                        else
                            SelectedIndex = count - 1;
                        break;

                    case ConsoleKey.Backspace:
                        EditProperty(true);
                        ////
                        break;

                    case ConsoleKey.Enter:
                        if (SelectedIndex == count - 1)
                        {
                            if (Finish()) return;
                        }
                        else
                        {
                            EditProperty();
                            ////
                        }
                        break;

                    case ConsoleKey.Escape:
                        foreach (var p in Properties)
                        {
                            Helper.mb(p.property.GetValue(Obj), " == ", p.initialValue, " => ", p.property.GetValue(Obj).Equals(p.initialValue));
                            if (!p.property.GetValue(Obj).Equals(p.initialValue))
                                p.property.SetValue(Obj, p.initialValue);
                        }
                        return;
                }
            }
        }
    }
}
