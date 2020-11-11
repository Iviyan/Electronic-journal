using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Electronic_journal
{
    public static class Helper
    {
        public static string[] Split(string str, int length)
        {
            if (str.Length <= length) return new string[] { str };

            List<string> res = new List<string>();
            for (int i = 0; i < str.Length; i += length)
            {
                if (i + length <= str.Length)
                    res.Add(str.Substring(i, length));
                else
                    res.Add(str.Substring(i));
            }

            return res.ToArray();
        }

        public static string ArrayToStr<T>(T[] arr)
        {
            if (arr.Length == 0) return "[]";
            string s = "[";
            foreach (T i in arr) s += i.ToString() + ", ";
            return s.Substring(0, s.Length - 2) + "]";
        }

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr h, string m, string c, int type);

        public static int MessageBox(string msg, string caption, int type) => MessageBox(IntPtr.Zero, msg, caption, type);
        public static int MessageBox(string msg, string caption) => MessageBox(IntPtr.Zero, msg, caption, 0);
        public static int mb(params object[] msg) => MessageBox(IntPtr.Zero, msg.Aggregate("", (string acc, object str) => acc += str.ToString()), "", 0);
    }
}
