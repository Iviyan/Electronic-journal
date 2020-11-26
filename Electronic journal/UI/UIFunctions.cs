using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electronic_journal
{
    public static class UIFunctions
    {
        class PasswordEditor
        {
            [Editor("Пароль"), StringParams(AllowEmpty = false)]
            public string Password { get; set; }
            public PasswordEditor(string pass) => Password = pass;
        }
        public static bool ChangePassword(ref string password, int startY = 0)
        {
            var passEditor = new PasswordEditor(password);
            ClassEditor<PasswordEditor> editor = new ClassEditor<PasswordEditor>(
                passEditor,
                startY
            );
            bool changed = editor.Edit();
            editor.Clear();
            if (changed)
            {
                password = passEditor.Password;
                return true;
            }
            return false;
        }

        public static string GetInitials(Person p) => $"{p.LastName} {p.FirstName[0]}.{p.Patronymic[0]}.";
        public static string GetFullName(Person p) => $"{p.LastName} {p.FirstName} {p.Patronymic}";
    }
}
