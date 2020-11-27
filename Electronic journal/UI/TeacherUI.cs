using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static Electronic_journal.Account;

namespace Electronic_journal
{
    public class TeacherUI
    {
        Teacher CurrentAccount;
        Settings settings;
        ConsoleSelect menu;

        string[] mainMenu = new string[]
                {
                    "Группы",
                    "Сменить пароль",
                    "Выход"
                };

        public TeacherUI(Teacher account, Settings settings)
        {
            this.settings = settings;
            CurrentAccount = account;

            menu = new ConsoleSelect(
                mainMenu,
                startY: 2,
                write: false
            );

            ShowMainMenu();
            Console.SetCursorPosition(0, 0);
        }
        void ShowMainMenu()
        {
            int selectedIndex = 0;
            for (; ; )
            {
                ConsoleHelper.WriteCenter($"{UIFunctions.GetFullName(CurrentAccount)} - преподаватель");
                menu.Clear();
                menu.StartY = 2;
                menu.Update(mainMenu, false);
                selectedIndex = menu.Choice(selectedIndex);
                if (selectedIndex != 1)
                {
                    ConsoleHelper.ClearArea(0, 0, Console.WindowWidth - 1, 0);
                    menu.Clear();
                    menu.StartY = 0;
                }

                switch (selectedIndex)
                {
                    case 0:
                        ShowGroupsMenu();
                        break;
                    case 1:
                        string pass = CurrentAccount.Password;
                        if (UIFunctions.ChangePassword(ref pass, 6))
                        {
                            CurrentAccount.Password = pass;
                            using (BinaryWriter writer = settings.GetAccountFileWriter(CurrentAccount.Login))
                                CurrentAccount.Export(writer);
                        }
                        Console.SetCursorPosition(0, 0);
                        break;
                    case 2: return;
                }
            }
        }

        void ShowGroupsMenu()
        {
            menu.Update(CurrentAccount.Groups.Add("Назад"));
            int selectedIndex = 0;
            for (; ; )
            {
                selectedIndex = menu.Choice(
                    selectedIndex,
                    (ConsoleKeyInfo key, int selectedIndex) =>
                    {
                        if (key.Key == ConsoleKey.Escape) return -1;
                        return null;
                    }, true
                );
                if (selectedIndex >= 0 && selectedIndex != CurrentAccount.Groups.Length)
                {
                    menu.Clear();
                    Group group = new Group(settings, CurrentAccount.Groups[selectedIndex]);
                    JournalEditor editor = new JournalEditor(group, CurrentAccount, settings);
                    if (editor.Edit())
                        group.ExportJournal(settings);
                    //editor.Clear();

                }
                else
                    break;
            }
        }
    }
}