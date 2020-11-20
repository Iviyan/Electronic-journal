using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electronic_journal
{
    public class StringArrayEditor
    {
        public IList<string> Array;
        public bool InitArrayIsReadOnly;
        public string Title;
        public int StartY;
        int FirstIndexY;

        public bool AllowEscape;

        public string[] GetArray() => Array.ToArray();

        private int selectedIndex;
        int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                Select(selectedIndex, selectedIndex < Array.Count ? ':' : ' ');
                selectedIndex = value;
                Select(selectedIndex, '>');
            }
        }
        int ChoicesCount = 1;

        int ChoiceSaveIndex;
        int ChoiceCancelIndex;

        public StringArrayEditor(IList<string> array, string title, int startY, bool allowEscape = false)
        {
            InitArrayIsReadOnly = array.IsReadOnly;
            if (array.IsReadOnly)
                Array = new List<string>(array);
            else
                Array = array;

            Title = title;
            StartY = startY;
            FirstIndexY = Title != null ? StartY + 1 : StartY;

            if (allowEscape)
            {
                if (!InitArrayIsReadOnly) throw new Exception("Возможность отмены недоступна для изменяемых коллекций");
                AllowEscape = allowEscape;
            }
        }
        public StringArrayEditor(IList<string> array, int startY, bool allowEscape = false) : this(array, null, startY, allowEscape) { }
        public StringArrayEditor(int startY, bool allowEscape = true)
        {
            Array = new List<string>();
            StartY = startY;
            FirstIndexY = Title != null ? StartY + 1 : StartY;
            AllowEscape = allowEscape;
        }

        int NumberLength(int num)
        {
            int i = 0;
            do
            {
                i++;
                num = num / 10;
            } while (num != 0);
            return i;
        }

        void Select(int index, char c = '>')
        {
            Console.SetCursorPosition(index < Array.Count ? NumberLength(index) : 0, FirstIndexY + index);
            Console.Write(c);
            if (Array.Count != 0)
                if (index < Array.Count)
                    Console.CursorLeft = NumberLength(index) + 2 + Array[index].Length;
                else
                    Console.CursorLeft = " Сохранить".Length;
            else
                Console.CursorLeft = 2;
        }
        void Write(int startIndex = 0)
        {
            Console.SetCursorPosition(0, FirstIndexY + startIndex);
            if (Array.Count != 0)
                for (int i = startIndex; i < Array.Count; i++)
                    Console.WriteLine($"{i}: {Array[i]}");
            else
                Console.WriteLine(">_");
            ChoiceSaveIndex = Math.Max(Array.Count, 1);
            Console.WriteLine(" Сохранить");
            if (AllowEscape)
            {
                Console.WriteLine(" Отмена");
                ChoiceCancelIndex = Array.Count + 1;
            }
        }
        public void Clear()
        {
            Console.CursorLeft = 0;
            ConsoleHelper.ClearArea(0, StartY, Console.WindowWidth, StartY + ChoicesCount + (Title != null ? 1 : 0));
        }
        void Clear(int startIndex)
        {
            Console.CursorLeft = 0;
            ConsoleHelper.ClearArea(0, FirstIndexY + startIndex, Console.WindowWidth, FirstIndexY + ChoicesCount + 1);
        }

        public bool Edit()
        {
            string[] initArr = null;
            if (AllowEscape)
                initArr = GetArray();

            int yPos = StartY;
            if (Title != null)
            {
                Console.SetCursorPosition(0, StartY);
                Console.WriteLine(Title);
                yPos++;
            }

            ChoicesCount = Math.Max(Array.Count, 1) + 1 + (AllowEscape ? 1 : 0);
            Write();
            Select(0);

            void Down()
            {
                if (SelectedIndex + 1 < ChoicesCount) SelectedIndex++;
                else SelectedIndex = 0;
            }
            void Up()
            {
                if (SelectedIndex > 0) SelectedIndex--;
                else SelectedIndex = ChoicesCount - 1;
            }

            Reader.ReadLineCallback ReadLineCallback = (ConsoleKeyInfo key) =>
            {
                if (key.Key == ConsoleKey.DownArrow)
                    return Reader.ReadLineCallbackResult.Enter;

                if (key.Key == ConsoleKey.UpArrow)
                    return Reader.ReadLineCallbackResult.Enter;

                return Reader.ReadLineCallbackResult.Continue;
            };
            bool ReadLineResult(ConsoleKeyInfo key) => key.Key == ConsoleKey.Enter || ReadLineCallback(key) == Reader.ReadLineCallbackResult.Enter;

            void NextAction(ConsoleKeyInfo key)
            {
                if (key.Key == ConsoleKey.Enter) Insert();
                if (key.Key == ConsoleKey.DownArrow) Down();
                if (key.Key == ConsoleKey.UpArrow) Up();
            }

            void Insert()
            {
                Array.Insert(SelectedIndex + 1, "");
                Clear(SelectedIndex + 1);
                Write(SelectedIndex + 1);
                SelectedIndex++;
                string str = Array[SelectedIndex];
                ConsoleKeyInfo key = Reader.ReadLine_esc_key(ref str, "", false, ReadLineCallback);
                bool esc = ReadLineResult(key);
                if (esc == false)
                {
                    Clear(SelectedIndex);
                    Array.RemoveAt(SelectedIndex);
                    Write(SelectedIndex);
                    Select(--selectedIndex);

                }
                else
                    Array[SelectedIndex] = str;

                ChoicesCount = Array.Count + 1;
                NextAction(key);
            }
            void AddFirstElement(string startValue = "")
            {
                Console.CursorLeft = 0;
                Console.Write("0> ");
                string str = "";
                ConsoleKeyInfo key = Reader.ReadLine_esc_key(ref str, startValue, false, ReadLineCallback);
                bool esc = ReadLineResult(key);
                if (esc == false)
                {
                    Console.CursorLeft = 0;
                    Console.Write(">_");
                    if (!String.IsNullOrEmpty(startValue))
                    {
                        Console.Write(new string(' ', startValue.Length + 1));
                        Console.CursorLeft = 2;
                    }
                }
                else
                    Array.Add(str);

                ChoicesCount = Array.Count + 1;
                NextAction(key);
            }

            ConsoleKeyInfo info;
            while (true)
            {
                info = Console.ReadKey(true);
                switch (info.Key)
                {
                    case ConsoleKey.DownArrow:
                        Down();
                        break;
                    case ConsoleKey.UpArrow:
                        Up();
                        break;
                    case ConsoleKey.Enter when SelectedIndex < Array.Count:
                        Insert();
                        break;
                    case ConsoleKey.Enter when Array.Count == 0 && SelectedIndex == 0:
                        AddFirstElement();
                        break;
                    case ConsoleKey.Delete when SelectedIndex < Array.Count && Array.Count != 0:
                        Clear(SelectedIndex);
                        Array.RemoveAt(SelectedIndex);
                        Write(SelectedIndex);
                        Select(SelectedIndex);

                        ChoicesCount = Array.Count + 1;
                        break;
                    case ConsoleKey.Enter when SelectedIndex == ChoiceSaveIndex:
                        return true;
                    case ConsoleKey.Enter when AllowEscape && SelectedIndex == ChoiceCancelIndex:
                    case ConsoleKey.Escape when AllowEscape:
                        Array = initArr.ToList();
                        Clear(0);
                        Write();
                        return false;
                    case ConsoleKey.Backspace when Array.Count > 0:
                        {
                            string str = Array[SelectedIndex];
                            Console.CursorLeft--; Console.Write(' ');
                            Console.CursorLeft = NumberLength(SelectedIndex) + 2;
                            ConsoleKeyInfo key = Reader.ReadLine_esc_key(ref str, str.Substring(0, str.Length - 1), false, ReadLineCallback);
                            bool esc = ReadLineResult(key);
                            if (esc == false)
                                Console.Write(str[str.Length - 1]);
                            else
                                Array[SelectedIndex] = str;

                            NextAction(key);
                        }
                        break;

                    default:
                        
                        if (Reader.IsInputChar(info.KeyChar))
                        {
                            if (Array.Count != 0)
                            {
                                string str = Array[SelectedIndex];
                                Console.CursorLeft = NumberLength(SelectedIndex) + 2;
                                ConsoleKeyInfo key = Reader.ReadLine_esc_key(ref str, str + info.KeyChar, false, ReadLineCallback);
                                bool esc = ReadLineResult(key);
                                if (esc == false)
                                {
                                    Console.CursorLeft = NumberLength(SelectedIndex) + 2 + Array[SelectedIndex].Length;
                                    Console.Write(' ');
                                    Console.CursorLeft--;
                                }
                                else
                                    Array[SelectedIndex] = str;

                                NextAction(key);
                            } else if (SelectedIndex == 0)
                                AddFirstElement(info.KeyChar.ToString());

                        }
                        break;
                }
            }
        }
    }
}