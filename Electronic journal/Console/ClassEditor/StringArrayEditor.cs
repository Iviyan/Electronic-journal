using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electronic_journal
{
    public class StringArrayEditor
    {
        public List<string> Array;
        public string Title;
        public int StartY;
        int FirstIndexY;

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

        public StringArrayEditor(string[] array, string title, int startY)
        {
            Array = new(array);
            Title = title;
            StartY = startY;
            FirstIndexY = Title != null ? StartY + 1 : StartY;
        }
        public StringArrayEditor(string[] array, int startY) : this(array, null, startY) { }
        public StringArrayEditor(int startY)
        {
            Array = new();
            StartY = startY;
            FirstIndexY = Title != null ? StartY + 1 : StartY;
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
            Console.WriteLine(" Сохранить");
        }
        public void Clear()
        {
            Console.CursorLeft = 0;
            ConsoleHelper.ClearArea(0, StartY, Console.WindowWidth, Array.Count + 1);
        }
        void Clear(int startIndex)
        {
            Console.CursorLeft = 0;
            ConsoleHelper.ClearArea(0, FirstIndexY + startIndex, Console.WindowWidth, Array.Count + 1);
        }

        public string[] Edit()
        {
            int yPos = StartY;
            if (Title != null)
            {
                Console.WriteLine(Title);
                yPos++;
            }

            ChoicesCount = Array.Count + 1;
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
                    case ConsoleKey.Enter when SelectedIndex == ChoicesCount - 1:
                        return Array.ToArray();

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
                                    Console.Write(" ");
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