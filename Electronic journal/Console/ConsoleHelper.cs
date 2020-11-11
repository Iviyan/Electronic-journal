using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Electronic_journal
{
    public static class ConsoleHelper
    {
        public static void ClearArea(int x1, int y1, int x2, int y2)
        {
            var pos = (Console.CursorLeft, Console.CursorTop);
            int width = x2 - x1 + 1;
            for (; y1 <= y2; y1++)
            {
                Console.SetCursorPosition(x1, y1);
                Console.Write(new string(' ', width));
            }
            Console.SetCursorPosition(pos.CursorLeft, pos.CursorTop);
        }


        public static void WriteCenter(string text, int x1 = 0, int length = -1, char fillLeft = ' ', char fillRight = ' ')
        {
            if (length == -1) length = Console.WindowWidth;
            if (text.Length > length) { text = text.Substring(0, length); Console.CursorLeft = x1; Console.Write(text); }
            int startX = x1 + length / 2 - (int)Math.Ceiling(text.Length / 2d);
            if (fillLeft != ' ') { Console.CursorLeft = x1; Console.Write(new string(fillLeft, startX - x1)); }
            else Console.CursorLeft = startX;
            Console.Write(text);
            if (fillRight != ' ') { Console.Write(new string(fillRight, length - (startX - x1 + text.Length))); }
        }
    }
}
