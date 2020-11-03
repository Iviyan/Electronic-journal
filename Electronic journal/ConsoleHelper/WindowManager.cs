using System;
using System.Collections.Generic;
using System.Text;

namespace Electronic_journal
{
    public class WindowManager
    {
        public int Count { get; private set; } = 0;
        private Dictionary<string, Window> Windows;

        public WindowManager()
        {
            Windows = new Dictionary<string, Window>();
        }

        public Window this[string name]
        {
            get => Windows[name];
            set => Windows[name] = value;
        }
        public void Add(Window window)
        {
            if (Windows.ContainsKey(window.Name)) throw new ArgumentException("A window with the same name has already been added");
            Windows.Add(window.Name, window);
            Count++;
        }
        public void Remove(string windowName)
        {
            if (Windows.Remove(windowName))
                Count--;
        }


    }
}
