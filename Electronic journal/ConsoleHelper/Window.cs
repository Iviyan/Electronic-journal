using System;
using System.Collections.Generic;
using System.Text;

namespace Electronic_journal
{
    public class Window
    {
        public enum WindowMode
        {
            FullScreen = 0,
            Fixed = 1
        }

        [Flags]
        public enum WindowAlign
        {
            Center = 1,
            Left = 2,
            Right = 4,
            Top = 8,
            Bottom = 16
        }
        public WindowMode Mode { get; set; }
        public WindowAlign Align { get; set; }

        public bool Visible { get; set; } = true;

        public string Name { get; set; }

        public int Margin { 
            set
            {
                MarginTop = MarginRight = MarginBottom = MarginLeft = value;
            }
        }
        public int MarginTop { get; set; }
        public int MarginRight { get; set; }
        public int MarginBottom { get; set; }
        public int MarginLeft { get; set; }

        //public Layout Layout { get; set; }

        public Window(string name)
        {
            Name = name;
            Mode = WindowMode.FullScreen;

        }

        public void Redraw()
        {

        }
    }
}
