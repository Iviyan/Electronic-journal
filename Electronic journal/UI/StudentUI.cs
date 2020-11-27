using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static Electronic_journal.Account;

namespace Electronic_journal
{
    public class StudentUI
    {
        Student CurrentAccount;
        Settings settings;
        ConsoleSelect menu;

        string[] mainMenu = new string[]
                {
                    "Журнал",
                    "Сменить пароль",
                    "Выход"
                };

        public StudentUI(Student account, Settings settings)
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
                ConsoleHelper.WriteCenter($"{UIFunctions.GetFullName(CurrentAccount)} - студент");
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
                        ShowJournal();
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

        void ShowJournal()
        {
            Group group = new Group(settings, CurrentAccount.Group);
            JournalViewer viewer = new(group, CurrentAccount);
            viewer.View();
            //viewer.Clear();
        }
    }
}