using System;
using System.Collections.Generic;
using System.Text;

namespace Electronic_journal
{
    public static class Reader
    {
        public delegate bool TestInputChar(char c);
        public static bool IsInputChar(char c) => Char.IsLetterOrDigit(c) || Char.IsPunctuation(c) || Char.IsWhiteSpace(c) || Char.IsSymbol(c);

        public static string ReadLine_esc(string input = "", bool newLine = true)
        {
            string v = input;
            ReadLine_esc(ref v, input, newLine);
            return v;
        }

        public static bool ReadLine_esc(ref string value, string input = "", bool newLine = true)
        {
            int left = Console.CursorLeft,
                pos = 0;

            StringBuilder buffer = new StringBuilder();

            if (input != "")
            {
                buffer.Append(input);
                pos = input.Length;
                Console.Write(input);
            }

            ConsoleKeyInfo key = Console.ReadKey(true);
            while (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Escape)
            {
                if (key.Key == ConsoleKey.Backspace && pos > 0)
                {
                    buffer.Remove(--pos, 1);
                    Console.CursorLeft--;
                    Console.Write(buffer.ToString(pos, buffer.Length - pos) + ' ');
                    Console.CursorLeft = left + pos;
                }
                else if (IsInputChar(key.KeyChar))
                {
                    if (/*oneLine && */!(left + pos + 1 < Console.WindowWidth)) continue;
                    buffer.Insert(pos++, key.KeyChar);
                    Console.Write(buffer.ToString(pos - 1, buffer.Length - pos + 1));
                    Console.CursorLeft = left + pos;
                }
                else if (key.Key == ConsoleKey.LeftArrow && pos > 0)
                {
                    Console.CursorLeft--; pos--;
                }
                else if (key.Key == ConsoleKey.RightArrow && pos < buffer.Length)
                {
                    Console.CursorLeft++; pos++;
                }
                key = Console.ReadKey(true);
            }

            if (key.Key == ConsoleKey.Enter)
            {
                if (newLine) Console.WriteLine();
                value = buffer.ToString();
                return true;
            }
            if (input != buffer.ToString())
            {
                Console.CursorLeft = left;
                Console.Write(input);
                if (input.Length < buffer.Length)
                    Console.Write(new string(' ', buffer.Length - input.Length));
                Console.CursorLeft = left + input.Length;
            }

            return false;
        }
    }
}
