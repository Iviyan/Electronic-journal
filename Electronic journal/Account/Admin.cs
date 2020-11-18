
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Electronic_journal
{
    public class Admin : Account, ICustomSerializable, IUI
    {
        public Admin(string login, string password) : base(login, password, AccountType.Admin) { }
        public Admin(BinaryReader reader) : base(reader, AccountType.Admin) { }

        public override void Export(BinaryWriter writer)
        {
            base.Export(writer);
        }

        //private delegate bool ValidateFunc
        public void UI(Settings settings)
        {
            string[] mainMenu = new string[]
                {
                    "Группы",
                    "Аккаунты",
                    "Добавить аккаунт",
                    "Добавить группу",
                    "Сменить пароль"
                };
            string[] groupsList = new string[] { };
            string[] accountsList = new string[] { };
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

            ConsoleSelect menu = new ConsoleSelect(
                mainMenu
            );
            void ShowMainMenu(bool updateMenu = true, int from = 0)
            {
                if (updateMenu) menu.Update(mainMenu);
                switch (menu.Choice(from))
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
                }
            }

            void ShowGroupsMenu(int from = 0)
            {
                UpdateGroupsList();
                menu.Update(groupsList);
                int sel = menu.Choice(
                    from,
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
                if (sel == -1) ShowMainMenu(from: 0);
                ShowGroupEditMenu(sel);
            }

            void ShowAccountsMenu(int from = 0)
            {
                UpdateAccountsList();
                menu.Update(accountsList);
                int sel = menu.Choice(
                    from,
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
                if (sel == -1) ShowMainMenu(from: 1);
                ShowAccountEditMenu(sel);
            }

            Regex regexLogin = new Regex(@"^[a-zA-Z0-9_]+$", RegexOptions.Compiled);
            Regex regexPassword = new Regex(@"^[a-zA-Z0-9\._]+$", RegexOptions.Compiled);

            ClassEditor<Account>.ValidateFunc validateLoginPassword = (Account account, string[] changedProperties, out string msg) =>
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
            };

            ClassEditor<Admin>.ValidateFunc validateAdmin =
                (Admin admin, string[] changedProperties, out string msg) => validateLoginPassword(admin, changedProperties, out msg);

            ClassEditor<Teacher>.ValidateFunc validateTeacher = (Teacher teacher, string[] changedProperties, out string msg) =>
            {
                if (!validateLoginPassword(teacher, changedProperties, out msg)) return false;
                msg = ""; return true;
            };

            // TODO: Сделать проверку на то, существует ли группа
            ClassEditor<Student>.ValidateFunc validateStudent = (Student student, string[] changedProperties, out string msg) =>
            {
                if (!validateLoginPassword(student, changedProperties, out msg)) return false;
                msg = ""; return true;
            };

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
                ShowAccountsMenu(index);
            } //ShowAccountEditMenu/

            void ShowAccountAddMenu()
            {
                menu.Update(accountAddMenu);
                Account account;
                int sel = menu.Choice(
                    (ConsoleKeyInfo key, int selectedIndex) =>
                    {
                        if (key.Key == ConsoleKey.Escape) return -1;
                        return null;
                    }
                );
                if (sel >= 0)
                {
                    menu.Clear();
                    switch (sel)
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
                ShowMainMenu(from: 2);
            } //ShowAccountAddMenu/

            ClassEditor<Group>.ValidateFunc validateGroup = (Group group, string[] changedProperties, out string msg) =>
            {
                msg = ""; return true;
            };

            void ShowGroupAddMenu()
            {
                menu.Clear();
                Group group = new Group("");
                ClassEditor<Group> editor = new ClassEditor<Group>(
                    group,
                    validateGroup
                );
                if (editor.Edit())
                    settings.AddGroup(group);
                editor.Clear();

                ShowMainMenu(from: 3);
            }

            void ShowGroupEditMenu(int index)
            {
                menu.Update(groupEditMenu);
                int sel = menu.Choice(
                    (ConsoleKeyInfo key, int selectedIndex) =>
                    {
                        if (key.Key == ConsoleKey.Escape) return -1;
                        return null;
                    }
                );
                if (sel >= 0)
                {
                    menu.Clear();
                    Group group = new Group(settings, groupsList[index]);
                    string name = group.Name;

                    switch (sel)
                    {
                        case 0: //Info
                            {
                                ClassEditor<Group> editor = new ClassEditor<Group>(
                                    group,
                                    validateGroup
                                );
                                if (editor.Edit())
                                    settings.AddGroup(group);
                                editor.Clear();
                            }
                            break;
                        case 1: //Journal
                            {

                            }
                            break;
                    }
                }
                ShowGroupsMenu(index);

            }


            ShowMainMenu(false);
        }
    }
}