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
        public enum WindowAlign
        {
            Center = 0,
            TopLeft = 1,
            Top = 2,
            TopRight = 3,
            Right = 4,
            BottomRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            Left = 8
        }
        public WindowMode Mode;
        public WindowAlign Align;

        public bool Visible;


    }
}
