using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Electronic_journal
{
    public class ClassEditor<T>
    {
        public readonly T Obj;
        public (PropertyInfo property, string name, bool readOnly)[] Properties;
        public ValidateFunc Validate;
        public ICustomEditor<T>[] CustomEditors;

        public int StartY;

        int ChoicesCount;
        int selectedIndex = 0;
        int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (ReadOnlyPropertyDataHeight > 0) ClearReadOnlyProperty();
                if (InputError)
                {
                    int startX = 2 + Properties[selectedIndex].name.Length + 2,
                        maxLength = Console.WindowWidth - 1 - startX;
                    string sval = GetStringValue(Properties[selectedIndex].property);
                    Console.CursorLeft = startX;
                    WriteValue(sval, maxLength);
                    int length = Console.CursorLeft - startX;
                    if (ValuesLength[selectedIndex] > length)
                        Console.Write(new string(' ', ValuesLength[selectedIndex] - length));
                    ClearError();
                    InputError = false;
                }

                Select(selectedIndex, ' ');
                selectedIndex = value;
                Select(selectedIndex);
                SetCursorPosition();
            }
        }

        int[] ValuesLength;

        (bool isValueType, object value)[] Changes;

        bool InputError = false;

        static bool validateTrue(T obj, string[] changedProperties, out string msg)
        {
            msg = ""; return true;
        }

        //public delegate bool ValidateFunc(T obj, out string msg);
        public delegate bool ValidateFunc(T obj, string[] changedProperties, out string msg);
        public ClassEditor(T obj, int startY = 0) : this(obj, validateTrue, new ICustomEditor<T>[] { }, startY) { }
        public ClassEditor(T obj, ValidateFunc validate, int startY = 0) : this(obj, validate, new ICustomEditor<T>[] { }, startY) { }
        public ClassEditor(T obj, ValidateFunc validate, ICustomEditor<T>[] additionalEditors, int startY = 0)
        {
            Obj = obj;
            Validate = validate;
            StartY = startY;

            var properties = new List<(PropertyInfo, string, bool)>();
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

                properties.Insert(ind++, (property, attribute.DisplayValue, attribute.ReadOnly));
            }
            Properties = properties.ToArray();
            ValuesLength = new int[properties.Count + additionalEditors.Length];

            Changes = new (bool, object)[Properties.Length];
            for (int i = 0; i < Properties.Length; i++)
            {
                bool isValueType = Properties[i].property.PropertyType.IsValueType || Properties[i].property.PropertyType == typeof(string);
                Changes[i] = (isValueType, isValueType ? Properties[i].property.GetValue(obj) : false);
            }

            CustomEditors = additionalEditors;

            ChoicesCount = Properties.Length + CustomEditors.Length + 1;
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
        string GetStringValueFromStringArray(IList<string> stringArray)
        {
            if (stringArray.Count == 0) return "[]";
            string s = "[";
            foreach (string i in stringArray) s += i.ToString() + ", ";
            return s.Substring(0, s.Length - 2) + "]";
        }
        string GetStringValue(PropertyInfo property)
        {
            var value = property.GetValue(Obj);

            switch (value)
            {
                case DateTime: return GetStringValueFromDateTime(property);
                case IList<string> val: return GetStringValueFromStringArray(val);
            }

            return value.ToString();
        }

        int WriteValue(string val, int maxLength)
        {
            int startX = Console.CursorLeft;
            if (val.Length > maxLength)
            {
                Console.Write(val.Substring(0, maxLength - 3));
                using (new UseConsoleColor(ConsoleColor.Red))
                    Console.Write("...");
                Helper.mb(Console.CursorLeft);
            }
            else
                Console.Write(val);
            return Console.CursorLeft - startX;
        }

        void Write()
        {
            Console.SetCursorPosition(0, StartY);
            for (int i = 0; i < Properties.Length; i++)
            {
                var p = Properties[i];
                Console.Write($"  {p.name}: ");
                int valueMaxLength = Console.WindowWidth - (p.name.Length + 4);
                string value = GetStringValue(p.property);
                if (p.readOnly)
                    using (new UseConsoleColor(ConsoleColor.DarkGray))
                        ValuesLength[i] = WriteValue(value, valueMaxLength);
                else
                    ValuesLength[i] = WriteValue(value, valueMaxLength);
                Console.CursorLeft = 0;
                Console.CursorTop++;
            }
            for (int i = 0; i < CustomEditors.Length; i++)
            {
                var editor = CustomEditors[i];
                Console.Write($"  {editor.PropertyName}: ");
                int valueMaxLength = Console.WindowWidth - (editor.PropertyName.Length + 4);
                string value = editor.GetStringValue();
                ValuesLength[i + Properties.Length] = WriteValue(value, valueMaxLength);

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
            if (SelectedIndex < Properties.Length + CustomEditors.Length)
            {
                int pos = (SelectedIndex < Properties.Length ? Properties[SelectedIndex].name.Length : CustomEditors[selectedIndex - Properties.Length].PropertyName.Length) + 4 + ValuesLength[SelectedIndex];
                if (pos < Console.WindowWidth)
                    Console.CursorLeft = pos;
                else
                    Console.CursorLeft = Console.WindowWidth - 1;
            }
            else
                Console.CursorLeft = 0;
        }

        int errorMsgHeight = 0;
        void ClearError()
        {
            if (errorMsgHeight == 0) return;
            ConsoleHelper.ClearArea(1, StartY + ChoicesCount + 1, Console.WindowWidth - 1, StartY + Properties.Length + 1 + errorMsgHeight);
            errorMsgHeight = 0;
        }
        void WriteError(string msg)
        {
            ClearError();
            using (new CursonPosition(1, StartY + ChoicesCount + 1))
                //using (new UseConsoleColor(ConsoleColor.Red))
                Console.Write($"!> {msg}");
            errorMsgHeight = (int)Math.Ceiling($"!> {msg}".Length / (double)Console.WindowWidth);
        }
        void WriteErrorAndMark(string msg)
        {
            WriteError(msg);
            InputError = true;
        }

        int ReadOnlyPropertyDataHeight = 0;
        void ViewReadOnlyProperty()
        {
            var value = Properties[SelectedIndex].property.GetValue(Obj);
            switch (value)
            {
                case IList<string> val:
                    {
                        if (val.Count == 0) return;
                        ClearError();

                        int startY = StartY + ChoicesCount + 1;
                        Console.SetCursorPosition(0, startY);
                        foreach (string s in val)
                            Console.WriteLine($"> {s}");
                        ReadOnlyPropertyDataHeight = Console.CursorTop - startY;
                    }
                    break;
            }
        }
        void ClearReadOnlyProperty()
        {
            if (ReadOnlyPropertyDataHeight == 0) return;
            int startY = StartY + ChoicesCount + 1;
            ConsoleHelper.ClearArea(0, startY, Console.WindowWidth - 1, startY + ReadOnlyPropertyDataHeight - 1);
            ReadOnlyPropertyDataHeight = 0;
        }
        void EditAdditionalProperty()
        {
            int customEditorIndex = selectedIndex - Properties.Length;
            var editor = CustomEditors[customEditorIndex];

            bool edited = editor.Edit(StartY + ChoicesCount + 2);
            editor.Clear();
            Console.CursorTop = StartY + SelectedIndex;
            if (edited)
            {
                Console.CursorLeft = 2 + editor.PropertyName.Length + 2;
                int valueMaxLength = Console.WindowWidth - (editor.PropertyName.Length + 4);
                string sval = editor.GetStringValue();

                ValuesLength[SelectedIndex] = WriteValue(sval, valueMaxLength);
            }
            else
                SetCursorPosition();
        }
        void EditProperty(bool backspace = false)
        {
            void setCursorBeforeInput() => Console.CursorLeft = 2 + Properties[selectedIndex].name.Length + 2;

            setCursorBeforeInput();

            void updateCurrentValue(string value)
            {
                setCursorBeforeInput();
                int valueMaxLength = Console.WindowWidth - (Properties[selectedIndex].name.Length + 4);

                int valueLength = WriteValue(value, valueMaxLength);
                if (valueLength < ValuesLength[SelectedIndex])
                    Console.Write(new string(' ', ValuesLength[SelectedIndex] - value.Length));

                ValuesLength[SelectedIndex] = valueLength;
            }

            void CheckBackspace()
            {
                if (backspace)
                {
                    updateCurrentValue("");
                    setCursorBeforeInput();
                }
            }

            var p = Properties[selectedIndex];
            var value = p.property.GetValue(Obj);

            string input = "";
            switch (value)
            {
                case byte:
                case int:
                    {
                        CheckBackspace();
                        string sval = value.ToString();
                        if (Reader.ReadLine_esc(ref input, backspace ? "" : sval, false))
                        {
                            bool parse = false;
                            switch (value) {
                                case int:
                                    parse = int.TryParse(input, out int intResult);
                                    updateCurrentValue(parse ? intResult.ToString() : sval);
                                    if (parse) p.property.SetValue(Obj, intResult);
                                    break;
                                case byte:
                                    parse = byte.TryParse(input, out byte byteResult);
                                    updateCurrentValue(parse ? byteResult.ToString() : sval);
                                    if (parse) p.property.SetValue(Obj, byteResult);
                                    break;
                            }
                            if (!parse)
                                WriteErrorAndMark("Ошибка ввода числа");
                        }
                        else if (backspace)
                            updateCurrentValue(sval);
                    }
                    break;
                case string val:
                    CheckBackspace();
                    if (Reader.ReadLine_esc(ref input, backspace ? "" : val, false))
                    {
                        p.property.SetValue(Obj, input);
                        ValuesLength[SelectedIndex] = input.Length;
                    }
                    else if (backspace)
                        updateCurrentValue(val);
                    break;
                case DateTime val:
                    {
                        CheckBackspace();
                        var attribute = p.property.GetCustomAttributes<DateTimeParamsAttribute>(true)
                                .SingleOrDefault();
                        string sval = GetStringValueFromDateTime(val, attribute);

                        if (Reader.ReadLine_esc(ref input, backspace ? "" : sval, false))
                        {
                            bool parse = DateTime.TryParse(input, out DateTime result);
                            updateCurrentValue(parse ? GetStringValueFromDateTime(result, attribute) : input);
                            if (parse)
                                p.property.SetValue(Obj, result);
                            else
                            {
                                WriteErrorAndMark("Ошибка ввода даты");
                            }
                        }
                        else if (backspace)
                            updateCurrentValue(sval);
                    }
                    break;
                case IList<string> val:
                    {
                        ClearError();

                        StringArrayEditor stringArrayEditor = new(val, $"{p.name}:", StartY + ChoicesCount + 2, val.IsReadOnly);
                        bool edited = stringArrayEditor.Edit();
                        Console.CursorTop = StartY + SelectedIndex;
                        if (edited)
                        {
                            if (val.IsReadOnly)
                                p.property.SetValue(Obj, stringArrayEditor.GetArray());
                            Changes[SelectedIndex].value = true;

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
            List<string> changedProperties = new();
            for (int i = 0; i < Properties.Length; i++)
            {
                if (Changes[i].isValueType)
                {
                    if (!Properties[i].property.GetValue(Obj).Equals(Changes[i].value))
                        changedProperties.Add(Properties[i].property.Name);
                }
                else
                    if ((bool)Changes[i].value)
                    changedProperties.Add(Properties[i].property.Name);
            }
            bool success = CheckFields(out msg) && Validate(Obj, changedProperties.ToArray(), out msg);
            if (success)
            {
                ClearError();
                Console.CursorTop = StartY + Properties.Length + 2;
                return true;
            }
            else
            {
                WriteError(msg);
                return false;
            }
        }

        public void Clear()
        {
            ClearError();
            ConsoleHelper.ClearArea(0, StartY, Console.WindowWidth - 1, StartY + Properties.Length + 1);
            Console.SetCursorPosition(0, StartY);
        }

        public bool Edit()
        {
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
                        if (SelectedIndex + 1 < ChoicesCount)
                            SelectedIndex++;
                        else
                            SelectedIndex = 0;
                        break;

                    case ConsoleKey.UpArrow:
                        if (SelectedIndex > 0)
                            SelectedIndex--;
                        else
                            SelectedIndex = ChoicesCount - 1;
                        break;

                    case ConsoleKey.Backspace when SelectedIndex < Properties.Length && !Properties[SelectedIndex].readOnly:
                        EditProperty(true);
                        break;

                    case ConsoleKey.Enter:
                        if (SelectedIndex == ChoicesCount - 1)
                        {
                            if (ReadOnlyPropertyDataHeight > 0) ClearReadOnlyProperty();
                            if (Finish()) return true;
                        }
                        else
                        {
                            if (SelectedIndex < Properties.Length)
                                if (Properties[SelectedIndex].readOnly)
                                    ViewReadOnlyProperty();
                                else
                                    EditProperty();
                            else
                                EditAdditionalProperty();

                        }
                        break;

                    case ConsoleKey.Escape:
                        if (ReadOnlyPropertyDataHeight > 0) ClearReadOnlyProperty();
                        for (int i = 0; i < Properties.Length; i++)
                        {
                            if (Changes[i].isValueType)
                                if (!Properties[i].property.GetValue(Obj).Equals(Changes[i].value))
                                    Properties[i].property.SetValue(Obj, Changes[i].value);
                        }
                        return false;
                }
            }
        }
    }
}
