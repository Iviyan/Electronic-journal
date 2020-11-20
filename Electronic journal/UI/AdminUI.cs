using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Electronic_journal.Account;

namespace Electronic_journal
{
    public class AdminUI
    {
        Admin CurrentAccount;
        Settings settings;
        ConsoleSelect menu;

        string[] mainMenu = new string[]
                {
                    "Группы",
                    "Аккаунты",
                    "Добавить аккаунт",
                    "Добавить группу",
                    "Сменить пароль"
                };
        string[] groupsList;
        string[] accountsList;
        void UpdateAccountsList() => accountsList = settings.GetAccountsList();
        void UpdateGroupsList() => groupsList = settings.GetGroupsList();
        string[] accountAddMenu = new string[]
            {
                    "Администратор",
                    "Преподаватель",
                    "Студент"
            };
        string[] groupEditMenu = new string[]
            {
                    "Информация",
                    "Журнал",
            };

        public AdminUI(Admin account, Settings settings)
        {
            this.settings = settings;
            CurrentAccount = account;

            menu = new ConsoleSelect(
                mainMenu
            );

            ShowMainMenu();
        }
        void ShowMainMenu()
        {
            int selectedIndex = 0;
            for (; ; )
            {
                menu.Update(mainMenu);
                selectedIndex = menu.Choice(selectedIndex);
                switch (selectedIndex)
                {
                    case 0:
                        ShowGroupsMenu();
                        break;
                    case 1:
                        ShowAccountsMenu();
                        break;
                    case 2:
                        ShowAccountAddMenu();
                        break;
                    case 3:
                        ShowGroupAddMenu();
                        break;
                    case 4:
                        string pass = CurrentAccount.Password;
                        if (UIFunctions.ChangePassword(ref pass, 6))
                        {
                            CurrentAccount.Password = pass;
                            using (BinaryWriter writer = settings.GetAccountFileWriter(CurrentAccount.Login))
                                CurrentAccount.Export(writer);
                        }
                        break;
                }
            }
        }

        void ShowGroupsMenu()
        {
            int selectedIndex = 0;
            for (; ; )
            {
                UpdateGroupsList();
                menu.Update(groupsList);
                selectedIndex = menu.Choice(
                    selectedIndex,
                    (ConsoleKeyInfo key, int selectedIndex) =>
                    {
                        if (key.Key == ConsoleKey.Escape) return -1;
                        if (key.Key == ConsoleKey.Delete)
                        {
                            settings.RemoveGroup(groupsList[selectedIndex]);
                            menu.Choices.RemoveAt(selectedIndex);
                        }
                        return null;
                    }
                );
                if (selectedIndex >= 0)
                    ShowGroupEditMenu(selectedIndex);
                else
                    break;
            }
        }

        void ShowAccountsMenu()
        {
            int selectedIndex = 0;
            for (; ; )
            {
                UpdateAccountsList();
                menu.Update(accountsList);
                selectedIndex = menu.Choice(
                    selectedIndex,
                    (ConsoleKeyInfo key, int selectedIndex) =>
                    {
                        if (key.Key == ConsoleKey.Escape) return -1;
                        if (key.Key == ConsoleKey.Delete)
                        {
                            settings.RemoveAccount(accountsList[selectedIndex]);
                            menu.Choices.RemoveAt(selectedIndex);
                        }
                        return null;
                    }
                );
                if (selectedIndex >= 0)
                    ShowAccountEditMenu(selectedIndex);
                else
                    break;
            }
        }

        static Regex regexLogin = new Regex(@"^[a-zA-Z0-9_]+$", RegexOptions.Compiled);
        static Regex regexPassword = new Regex(@"^[a-zA-Z0-9\._]+$", RegexOptions.Compiled);

        //ClassEditor<Account>.ValidateFunc
        bool validateLoginPassword(Account account, string[] changedProperties, out string msg)
        {
            if (changedProperties.Contains(nameof(account.Login)))
            {
                if (accountsList.Contains(account.Login))
                {
                    msg = "Аккаунт с таким логином уже существует";
                    return false;
                }
                if (!regexLogin.IsMatch(account.Login))
                {
                    msg = "Логин должен состоять из ";
                    return false;
                }
            }
            if (changedProperties.Contains(nameof(account.Password)) && !regexPassword.IsMatch(account.Password))
            {
                msg = "Пароль должен долержать только символы латинского алфавита, цифры, . и _";
                return false;
            }
            msg = ""; return true;
        }

        bool validateAdmin(Admin admin, string[] changedProperties, out string msg) => validateLoginPassword(admin, changedProperties, out msg);

        bool validateTeacher(Teacher teacher, string[] changedProperties, out string msg)
        {

            if (!validateLoginPassword(teacher, changedProperties, out msg)) return false;
            msg = ""; return true;
        }

        // TODO: Сделать проверку на то, существует ли группа
        bool validateStudent(Student student, string[] changedProperties, out string msg)
        {
            if (!validateLoginPassword(student, changedProperties, out msg)) return false;
            msg = ""; return true;
        }

        void ShowAccountEditMenu(int index)
        {
            Account account = settings.LoadAccount(accountsList[index]);
            string login = account.Login;
            menu.Clear();

            BinaryWriter RewriteFile() => settings.GetAccountFileWriter(login);

            switch (account.Type)
            {
                case AccountType.Admin:
                    {
                        ClassEditor<Admin> editor = new ClassEditor<Admin>(
                            (Admin)account,
                            validateAdmin
                        );
                        if (editor.Edit())
                            account.Export(RewriteFile());
                        editor.Clear();
                    }
                    break;
                case AccountType.Teacher:
                    {
                        ClassEditor<Teacher> editor = new ClassEditor<Teacher>(
                            (Teacher)account,
                            validateTeacher
                        );
                        if (editor.Edit())
                            account.Export(RewriteFile());
                        editor.Clear();
                    }
                    break;
                case AccountType.Student:
                    {
                        ClassEditor<Student> editor = new ClassEditor<Student>(
                            (Student)account,
                            validateStudent
                        );
                        if (editor.Edit())
                            account.Export(RewriteFile());
                        editor.Clear();
                    }
                    break;
            }
        }

        void ShowAccountAddMenu()
        {
            menu.Update(accountAddMenu);
            Account account;
            int selectedIndex = menu.Choice(
                (ConsoleKeyInfo key, int selectedIndex) =>
                {
                    if (key.Key == ConsoleKey.Escape) return -1;
                    return null;
                }
            );
            if (selectedIndex >= 0)
            {
                menu.Clear();
                switch (selectedIndex)
                {
                    case 0: //Admin
                        {
                            account = new Admin("", "");
                            ClassEditor<Admin> editor = new ClassEditor<Admin>(
                                (Admin)account,
                                validateAdmin
                            );
                            if (editor.Edit())
                                settings.AddAccount(account);
                            editor.Clear();
                        }
                        break;
                    case 1: //Teacher
                        {
                            account = new Teacher("", "", "", "", "", DateTime.Today, new string[0], new string[0]);
                            ClassEditor<Teacher> editor = new ClassEditor<Teacher>(
                                (Teacher)account,
                                validateTeacher
                            );
                            if (editor.Edit())
                                settings.AddAccount(account);
                            editor.Clear();
                        }
                        break;
                    case 2: //Student
                        {
                            account = new Student("", "", "", "", "", DateTime.Today, "1");
                            ClassEditor<Student> editor = new ClassEditor<Student>(
                                (Student)account,
                                validateStudent
                            );
                            if (editor.Edit())
                                settings.AddAccount(account);
                            editor.Clear();
                        }
                        break;
                }
            }
        }

        bool validateGroup(Group group, string[] changedProperties, out string msg)
        {
            if (changedProperties.Contains(nameof(group.Name)))
            {
                if (groupsList.Contains(group.Name))
                {
                    msg = "Группа с таким именем уже существует";
                    return false;
                }
            }
            msg = ""; return true;
        }

        void ShowGroupAddMenu()
        {
            menu.Clear();
            Group group = new Group("");
            ClassEditor<Group> editor = new ClassEditor<Group>(
                group,
                validateGroup,
                new ICustomEditor<Group>[]
                {
                    new TeachersAndDisciplinesEditor(group, "Дисциплины преподавателей")
                }
            );
            if (editor.Edit())
                settings.AddGroup(group);
            editor.Clear();
        }

        void ShowGroupEditMenu(int index)
        {
            int selectedIndex = 0;
            for (; ; )
            {
                menu.Update(groupEditMenu);
                selectedIndex = menu.Choice(
                    selectedIndex,
                    (ConsoleKeyInfo key, int selectedIndex) =>
                    {
                        if (key.Key == ConsoleKey.Escape) return -1;
                        return null;
                    }
                );
                if (selectedIndex >= 0)
                {
                    menu.Clear();
                    Group group = new Group(settings, groupsList[index]);
                    string name = group.Name;

                    switch (selectedIndex)
                    {
                        case 0: //Info
                            {
                                ClassEditor<Group> editor = new ClassEditor<Group>(
                                    group,
                                    validateGroup,
                                    new ICustomEditor<Group>[]
                                    {
                                        new TeachersAndDisciplinesEditor(group, "Дисциплины преподавателей")
                                    }
                                );
                                if (editor.Edit())
                                    group.Export(settings);
                                editor.Clear();
                            }
                            break;
                        case 1: //Journal
                            {

                            }
                            break;
                    }
                }
                else
                    break;
            }
        }
    }
}