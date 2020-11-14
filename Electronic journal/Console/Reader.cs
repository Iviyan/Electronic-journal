using System;
using System.Collections.Generic;
using System.Text;

namespace Electronic_journal
{
    public static class Reader
    {
        public enum ReadLineCallbackResult
        {
            Continue,
            Enter,
            Escape
        }
        public delegate ReadLineCallbackResult ReadLineCallback(ConsoleKeyInfo key);
        public static bool IsInputChar(char c) => Char.IsLetterOrDigit(c) || Char.IsPunctuation(c) || Char.IsWhiteSpace(c);// || Char.IsSymbol(c);

        public static string ReadLine_esc(string input = "", bool newLine = true, ReadLineCallback callback = null)
        {
            string v = input;
            ReadLine_esc(ref v, input, newLine, callback);
            return v;
        }

        public static bool ReadLine_esc(ref string value, string input = "", bool newLine = true, ReadLineCallback callback = null) =>
            ReadLine_esc_key(ref value, input, newLine, callback).Key == ConsoleKey.Enter;
        public static ConsoleKeyInfo ReadLine_esc_key(ref string value, string input = "", bool newLine = true, ReadLineCallback callback = null)
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

            bool callbackExists = callback != null;
            bool callbackEnter = false;

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
                else if (callbackExists)
                {
                    ReadLineCallbackResult res = callback(key);
                    if (res == ReadLineCallbackResult.Enter) { callbackEnter = true; break; }
                    if (res == ReadLineCallbackResult.Escape) break;
                }
                key = Console.ReadKey(true);
            }

            if (key.Key == ConsoleKey.Enter || callbackEnter)
            {
                if (newLine) Console.WriteLine();
                value = buffer.ToString();
                return key;
            }
            if (input != buffer.ToString())
            {
                Console.CursorLeft = left;
                Console.Write(input);
                if (input.Length < buffer.Length)
                    Console.Write(new string(' ', buffer.Length - input.Length));
                Console.CursorLeft = left + input.Length;
            }

            return key;
        }
    }
}
