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
        public List<(PropertyInfo property, string name, object initialValue)> Properties;
        ValidateFunc Validate;

        public delegate bool ValidateFunc(T obj, out string msg);
        public ClassEditor(T obj, ValidateFunc validate)
        {
            Obj = obj;
            Validate = validate;

            Properties = new List<(PropertyInfo, string, object)>();
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

                Properties.Insert(ind++, (property, attribute.DisplayValue, property.GetValue(obj)));
            }

        }

        string GetStringValueFromDateTime(PropertyInfo dateTimeProperty)
        {
            DateTime value = (DateTime)dateTimeProperty.GetValue(Obj);
            var dateTimeAttribute = dateTimeProperty.GetCustomAttributes<DateTimeModeAttribute>(true)
                                    .SingleOrDefault();
            if (dateTimeAttribute != null)
                return GetStringValueFromDateTime(value, dateTimeAttribute);

            return value.ToString();
        }
        string GetStringValueFromDateTime(DateTime dateTime, DateTimeModeAttribute attribute)
        {
            if (attribute.OnlyDate)
                return dateTime.ToString("d");

            return dateTime.ToString();
        }
        string GetStringValue(PropertyInfo property)
        {
            var value = property.GetValue(Obj);

            switch (value)
            {
                case DateTime val: return GetStringValueFromDateTime(property);
            }

            return value.ToString();
        }

        public void Edit()
        {
            int selectInd = 0;
            int top = Console.CursorTop;
            int count = Properties.Count + 1;
            Console.Write(Properties.Aggregate("", (acc, prop) => acc += $"  {prop.name}: {GetStringValue(prop.property)}\n") + "  Сохранить\n");
            ConsoleKeyInfo info;
            
            void setSel()
            {
                if (selectInd < count - 1)
                    Console.CursorLeft = Properties[selectInd].name.Length + 4 + GetStringValue(Properties[selectInd].property).Length;
                else
                    Console.CursorLeft = 0;
            }
            void select(int ind)
            {
                Console.SetCursorPosition(0, top + selectInd);
                Console.CursorLeft = 0;
                Console.Write($" ");
                Console.SetCursorPosition(0, top + ind);
                Console.Write($">");
                selectInd = ind;
                setSel();
            }

            int errorMsgHeight = 0;
            void clearError() => ConsoleHelper.ClearArea(1, top + count + 1, Console.WindowWidth - 1, top + count + 1 + errorMsgHeight);
            void writeError(string msg)
            {
                clearError();
                using (new CursonPosition(1, top + count + 1))
                    //using (new UseConsoleColor(ConsoleColor.Red))
                    Console.Write($"!> {msg}");
                errorMsgHeight = (int)Math.Ceiling($"!> {msg}".Length / (double)Console.WindowWidth);
            }
            bool end()
            {
                bool success = Validate(Obj, out string msg);
                if (success)
                {
                    clearError();
                    Console.CursorTop = top + count + 1;
                    return true;
                }
                else
                {
                    writeError(msg);
                    return false;
                }
            }

            select(0);

            void edit(bool backspace = false)
            {
                var p = Properties[selectInd];

                void setCursorBeforeInput() => Console.CursorLeft = 2 + Properties[selectInd].name.Length + 2;
                setCursorBeforeInput();

                var value = p.property.GetValue(Obj);

                void updateCurrentValue(string value, int inputLength)
                {
                    setCursorBeforeInput();
                    Console.Write(value);
                    if (value.Length < inputLength)
                        Console.Write(new string(' ', inputLength - value.Length));
                }

                if (backspace)
                {
                    updateCurrentValue("", value.ToString().Length);
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
                                updateCurrentValue(parse ? result.ToString() : sval, input.Length);
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
                            var attribute = p.property.GetCustomAttributes<DateTimeModeAttribute>(true)
                                    .SingleOrDefault();
                            string sval = GetStringValueFromDateTime(val, attribute);

                            if (Reader.ReadLine_esc(ref input, backspace ? "" : sval, false))
                            {
                                bool parse = DateTime.TryParse(input, out DateTime result);
                                updateCurrentValue(parse ? GetStringValueFromDateTime(result, attribute) : sval, input.Length);
                                if (parse)
                                    p.property.SetValue(Obj, result);
                                else
                                    writeError("Ошибка ввода даты");
                            }
                        }
                        break;
                }
            }

            while (true)
            {
                info = Console.ReadKey(true);

                switch (info.Key)
                {
                    case ConsoleKey.DownArrow:
                        if (selectInd + 1 < count)
                            select(selectInd + 1);
                        else
                            select(0);
                        break;

                    case ConsoleKey.UpArrow:
                        if (selectInd > 0)
                            select(selectInd - 1);
                        else
                            select(count - 1);
                        break;

                    case ConsoleKey.Backspace:
                        edit(true);
                        select(selectInd);
                        break;

                    case ConsoleKey.Enter:
                        if (selectInd == count - 1)
                        {
                            if (end()) return;
                        }
                        else
                        {
                            edit();
                            select(selectInd);
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

                setSel();

            }
        }
    }
}
