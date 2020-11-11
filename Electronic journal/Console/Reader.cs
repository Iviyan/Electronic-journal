using System;
using System.Collections.Generic;
using System.Text;

namespace Electronic_journal
{
    public static class Reader
    {
        public static bool IsInputChar(char c) => Char.IsLetterOrDigit(c) || Char.IsPunctuation(c) || Char.IsWhiteSpace(c) || Char.IsSymbol(c);

        public static string ReadLine_esc(string input = "")
        {
            string v = input;
            ReadLine_esc(ref v, input);
            return v;
        }
        public static bool ReadLine_esc(ref string value, string input = "")
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
                Console.WriteLine();
                value = buffer.ToString();
                return true;
            }
            return false;
        }
    }
}
