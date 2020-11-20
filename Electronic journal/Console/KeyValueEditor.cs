using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electronic_journal
{
    public class KeyValueEditor
    {
        public (string name, string value)[] Elements;
        public OnChangeEvent OnChange;
        public OnSelectEvent OnSelect;
        public int StartY;

        int selectedIndex = 0;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                Select(selectedIndex, ' ');
                selectedIndex = value;
                Select(selectedIndex);
                SetCursorPosition();

                if (onSelectEventExists)
                    using (new CursonPosition()) 
                        OnSelect(SelectedIndex < Elements.Length ? SelectedIndex : -1, this);
            }
        }
        int ChoicesCount;

        bool onChangeEventExists,
             onSelectEventExists;

        /// <summary>
        /// 
        /// </summary>
        /// <returns>string - replace value / null</returns>
        public delegate string OnChangeEvent(int changedIndex, string value, KeyValueEditor editor);

        /// <summary>
        /// selected index is menu element -> -1 else index
        /// </summary>
        public delegate void OnSelectEvent(int selectedIndex, KeyValueEditor editor);
        public KeyValueEditor((string name, string value)[] elements, OnChangeEvent onChange, OnSelectEvent onSelect, int startY = 0)
        {
            Elements = elements;
            OnChange = onChange;
            OnSelect = onSelect;
            StartY = startY;

            ChoicesCount = Elements.Length + 1;

            onChangeEventExists = OnChange != null;
            onSelectEventExists = OnSelect != null;
        }

        void Write()
        {
            Console.SetCursorPosition(0, StartY);
            foreach (var kv in Elements)
                Console.WriteLine($" {kv.name}:  {kv.value}");
            Console.WriteLine($" Сохранить");
        }
        void Select(int index, char c = '>')
        {
            Console.SetCursorPosition(0, StartY + index);
            Console.Write(c);
        }
        void SetCursorPosition()
        {
            if (SelectedIndex < Elements.Length)
            {
                int pos = 1 + Elements[SelectedIndex].name.Length + 3 + Elements[SelectedIndex].value.Length;
                if (pos < Console.WindowWidth)
                    Console.CursorLeft = pos;
                else
                    Console.CursorLeft = Console.WindowWidth - 1;
            }
            else
                Console.CursorLeft = 0;
        }

        public bool Edit(out (string name, string value)[] elements)
        {
            elements = null;
            Write();
            Select(0);
            SetCursorPosition();

            

            ConsoleKeyInfo info;
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

                    case ConsoleKey.Enter:
                        if (SelectedIndex == ChoicesCount - 1)
                        {
                            elements = Elements;
                            return true;
                        }
                        else
                        {
                            Console.CursorLeft = Elements[SelectedIndex].name.Length + 4;
                            if (Reader.ReadLine_esc(ref Elements[SelectedIndex].value, Elements[SelectedIndex].value, false))
                                if (onChangeEventExists)
                                {
                                    string s = null;
                                    using (new CursonPosition())
                                        s = OnChange(SelectedIndex, Elements[SelectedIndex].value, this);
                                    if (s != null)
                                    {
                                        Elements[selectedIndex].value = s;
                                        Console.CursorLeft = Elements[SelectedIndex].name.Length + 4;
                                        Console.Write(new string(' ', Elements[SelectedIndex].value.Length));
                                        Console.CursorLeft = Elements[SelectedIndex].name.Length + 4;
                                        Console.Write(s);
                                    }
                                }
                        }
                        break;

                    case ConsoleKey.Escape:
                        return false;
                }
            }
        }

        public void SelectColor(int elementIndex, ConsoleColor color)
        {
            using (new CursonPosition(Elements[SelectedIndex].name.Length + 4, StartY + elementIndex))
                using (new UseConsoleColor(color))
                    Console.Write(Elements[selectedIndex].value);
        }
        public void UpdateValue(int elementIndex, string value)
        {
            using (new CursonPosition(Elements[SelectedIndex].name.Length + 4, StartY + elementIndex))
            {
                Console.Write(new string(' ', Elements[elementIndex].value.Length));
                Console.CursorLeft -= Elements[elementIndex].value.Length;
                Console.Write(value);
            }
            Elements[elementIndex].value = value;
        }
        public void Clear()
        {
            ConsoleHelper.ClearArea(0, StartY, Console.WindowWidth - 1, StartY + Elements.Length + 1);
            Console.SetCursorPosition(0, StartY);
        }

    }
}
