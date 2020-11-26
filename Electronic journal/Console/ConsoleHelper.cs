using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Electronic_journal
{
    public static class ConsoleHelper
    {
        public static void ClearArea(int x1, int y1, int x2, int y2)
        {
            using (new CursonPosition())
            {
                int width = x2 - x1 + 1;
                for (; y1 <= y2; y1++)
                {
                    Console.SetCursorPosition(x1, y1);
                    Console.Write(new string(' ', width));
                }
            }
        }


        public static void WriteCenter(string text, int x1 = 0, int length = -1, char fillLeft = ' ', char fillRight = ' ')
        {
            if (length == -1) length = Console.WindowWidth;
            if (text.Length > length) { text = text.Substring(0, length); Console.CursorLeft = x1; Console.Write(text); }
            int startX = x1 + length / 2 - (int)Math.Ceiling(text.Length / 2d);
            if (startX < x1) startX = x1;
            if (fillLeft != ' ') { Console.CursorLeft = x1; Console.Write(new string(fillLeft, startX - x1)); }
            else Console.CursorLeft = startX;
            Console.Write(text);
            if (fillRight != ' ') { Console.Write(new string(fillRight, length - (startX - x1 + text.Length))); }
        }
    }
    public class CursonPosition : IDisposable
    {
        public int CursorLeft, CursorTop;
        public CursonPosition() => (CursorTop, CursorLeft) = (Console.CursorTop, Console.CursorLeft);
        public CursonPosition(int cursorLeft, int cursorTop)
        {
            (CursorLeft, CursorTop) = (Console.CursorLeft, Console.CursorTop);
            (Console.CursorLeft, Console.CursorTop) = (cursorLeft, cursorTop);
        }
        public void Dispose() => Console.SetCursorPosition(CursorLeft, CursorTop);
    }
    public class UseConsoleColor : IDisposable
    {
        public ConsoleColor Color;
        public UseConsoleColor(ConsoleColor color) => (Color, Console.ForegroundColor) = (Console.ForegroundColor, color);
        public void Dispose() => Console.ForegroundColor = Color;
    }
}
