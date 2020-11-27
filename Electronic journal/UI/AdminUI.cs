using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
                    "Сменить пароль",
                    "Выход"
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
                    "Назад",
            };

        public AdminUI(Admin account, Settings settings)
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
                ConsoleHelper.WriteCenter("Администратор");
                menu.Clear();
                menu.StartY = 2;
                menu.Update(mainMenu, false);
                selectedIndex = menu.Choice(selectedIndex);
                if (selectedIndex != 4)
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

                        if (UIFunctions.ChangePassword(ref pass, 2 + mainMenu.Length + 1))
                        {
                            CurrentAccount.Password = pass;
                            using (BinaryWriter writer = settings.GetAccountFileWriter(CurrentAccount.Login))
                                CurrentAccount.Export(writer);
                        }
                        Console.SetCursorPosition(0, 0);
                        break;
                    case 5: return;
                }
            }
        }

        void ShowGroupsMenu()
        {
            int selectedIndex = 0;
            for (; ; )
            {
                UpdateGroupsList();
                menu.Update(groupsList.Add("Назад"));
                selectedIndex = menu.Choice(
                    selectedIndex,
                    (ConsoleKeyInfo key, int selectedIndex) =>
                    {
                        if (key.Key == ConsoleKey.Escape) return -1;
                        if (key.Key == ConsoleKey.Delete && selectedIndex < groupsList.Length)
                        {
                            settings.RemoveGroup(groupsList[selectedIndex]);
                            menu.Choices.RemoveAt(selectedIndex);
                        }
                        return null;
                    }
                );
                if (selectedIndex == groupsList.Length) break;
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
                menu.Update(accountsList.Add("Назад"));
                selectedIndex = menu.Choice(
                    selectedIndex,
                    (ConsoleKeyInfo key, int selectedIndex) =>
                    {
                        if (key.Key == ConsoleKey.Escape) return -1;
                        if (key.Key == ConsoleKey.Delete && selectedIndex < accountsList.Length)
                        {
                            settings.RemoveAccount(accountsList[selectedIndex]);
                            menu.Choices.RemoveAt(selectedIndex);
                        }
                        return null;
                    }
                );
                if (selectedIndex == accountsList.Length) break;
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

        bool validateStudent(Student student, string[] changedProperties, out string msg)
        {
            if (!validateLoginPassword(student, changedProperties, out msg)) return false;

            if (student.Group != "")
            {
                if (groupsList == null) UpdateGroupsList();
                if (!groupsList.Contains(student.Group))
                {
                    msg = $"Группы {student.Group} не существует";
                    return false;
                }
                if (changedProperties.Contains(nameof(student.Group)))
                {
                    Group group = new Group(settings, student.Group);
                    if (!group.Students.Contains(student.Login))
                        group.Students.Add(student.Login);
                    group.Export(settings);
                }
            }

            msg = ""; return true;
        }

        void ShowAccountEditMenu(int index)
        {
            Account account = settings.LoadAccount(accountsList[index]);
            string _login = account.Login;
            menu.Clear();

            BinaryWriter RewriteFile() => settings.GetAccountFileWriter(_login);

            switch (account.Type)
            {
                case AccountType.Admin:
                    {
                        ClassEditor<Admin> editor = new ClassEditor<Admin>(
                            (Admin)account,
                            validateAdmin
                        );
                        if (editor.Edit())
                            using (var writer = RewriteFile())
                                account.Export(writer);
                        editor.Clear();
                    }
                    break;
                case AccountType.Teacher:
                    {
                        Teacher teacher = (Teacher)account;
                        ClassEditor<Teacher> editor = new ClassEditor<Teacher>(
                            (Teacher)account,
                            validateTeacher
                        );
                        if (editor.Edit())
                            using (var writer = RewriteFile())
                                account.Export(writer);
                        editor.Clear();

                        if (_login != teacher.Login)
                        {
                            foreach (string groupName in teacher.Groups)
                            {
                                Group group = new Group(settings, groupName);
                                group.Teachers[Array.IndexOf(group.Teachers.ToArray(), _login)] = teacher.Login;
                                group.ExportInfo(settings);
                            }
                        }
                    }
                    break;
                case AccountType.Student:
                    {
                        Student student = (Student)account;
                        string _group = student.Group;
                        ClassEditor<Student> editor = new ClassEditor<Student>(
                            student,
                            validateStudent
                        );
                        if (editor.Edit())
                            using (var writer = RewriteFile())
                                account.Export(writer);
                        editor.Clear();

                        if (_group != "" && student.Group != _group)
                        {
                            Group previousGroup = new Group(settings, _group);
                            if (previousGroup.Students.Contains(_login))
                            {
                                previousGroup.Students.Remove(_login);
                                previousGroup.Export(settings);
                            }
                            if (student.Group != "" && groupsList.Contains(student.Group))
                            {
                                Group newGroup = new Group(settings, student.Group);
                                newGroup.Students.Add(student.Login);
                                newGroup.Export(settings);
                            }
                        }
                        if (_login != student.Login)
                        {
                            Group group = new Group(settings, student.Group);
                            group.Students[Array.IndexOf(group.Students.ToArray(), _login)] = student.Login;
                            group.ExportInfo(settings);
                        }
                    }
                    break;
            }

            if (_login != account.Login)
                settings.RenameAccount(_login, account.Login);
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
                            account = new Student("", "", "", "", "", DateTime.Today, "");
                            Student student = (Student)account;

                            ClassEditor<Student> editor = new ClassEditor<Student>(
                                (Student)account,
                                validateStudent
                            );
                            if (editor.Edit())
                                settings.AddAccount(account);
                            editor.Clear();

                            if (student.Group != "" && groupsList.Contains(student.Group))
                            {
                                Group group = new Group(settings, student.Group);
                                group.Students.Add(student.Login);
                                group.Export(settings);
                            }
                        }
                        break;
                }
            }
        }

        // TODO: сделать проверку на: существование студентов, учителей, наличие у них дисциплин -> classEditor + Warnings
        bool validateGroupName(Group group, string[] changedProperties, out string msg)
        {
            if (changedProperties.Contains(nameof(group.Name)) && groupsList.Contains(group.Name))
            {
                msg = "Группа с таким именем уже существует";
                return false;

            }
            msg = ""; return true;
        }
        void SetStudentsGroup(IEnumerable<Student> students, string groupName)
        {
            foreach (var student in students)
            {
                student.Group = groupName;
                using (var writer = settings.GetAccountFileWriter(student.Login))
                    student.Export(writer);
            }
        }
        void SetStudentsGroup(IEnumerable<string> students, string groupName)
        {
            foreach (var studentLogin in students)
            {
                Student student = settings.LoadAccount(studentLogin) as Student;
                if (student == null) continue;
                student.Group = groupName;
                using (var writer = settings.GetAccountFileWriter(student.Login))
                    student.Export(writer);
            }
        }
        void SetTeachersGroupAndDisciplines(List<(Teacher teacher, byte[] disciplines)> teachers, Group group)
        {
            foreach (var teacher in teachers)
            {
                if (!teacher.teacher.Groups.Contains(group.Name))
                    teacher.teacher.Groups = teacher.teacher.Groups.Add(group.Name);

                foreach (byte d in teacher.disciplines)
                    if (!teacher.teacher.Disciplines.Contains(group.Disciplines[d]))
                        teacher.teacher.Disciplines = teacher.teacher.Disciplines.Add(group.Disciplines[d]);

                using (var writer = settings.GetAccountFileWriter(teacher.teacher.Login))
                    teacher.teacher.Export(writer);
            }
        }
        void UpdateTeachersGroupAndDisciplines(List<(Teacher teacher, byte[] _disciplines, byte[] disciplines)> teachers, Group group, string groupPreviousName = null)
        {
            foreach (var teacher in teachers)
            {
                if (groupPreviousName == null)
                {
                    if (!teacher.teacher.Groups.Contains(group.Name))
                        teacher.teacher.Groups = teacher.teacher.Groups.Add(group.Name); // TODO: убрать
                }
                else
                {
                    int index = Array.IndexOf(teacher.teacher.Groups, groupPreviousName);
                    teacher.teacher.Groups[index] = group.Name;
                }
                var newDisciplines = teacher.disciplines.Except(teacher._disciplines);//.ToArray();
                var deletedDisciplines = teacher._disciplines.Except(teacher.disciplines);//.ToArray();
                foreach (byte d in newDisciplines)
                    if (!teacher.teacher.Disciplines.Contains(group.Disciplines[d]))
                        teacher.teacher.Disciplines = teacher.teacher.Disciplines.Add(group.Disciplines[d]);

                teacher.teacher.Disciplines = teacher.teacher.Disciplines.Except(
                    ListDisciplinesToRemove(teacher.teacher, group.Name, deletedDisciplines.Select(b => group.Disciplines[b]))
                    ).ToArray();

                using (var writer = settings.GetAccountFileWriter(teacher.teacher.Login))
                    teacher.teacher.Export(writer);
            }
        }
        string[] ListDisciplinesToRemove(Teacher teacher, string groupName, IEnumerable<string> disciplines)
        {
            if (disciplines == null || !disciplines.Any()) return new string[] { };

            HashSet<string> Disciplines = new(disciplines);
            var groups = teacher.Groups.Except(new string[] { groupName });
            foreach (string group in groups)
            {
                string[] disciplines_ = Group.GetTeacherDisciplines(settings, group, teacher.Login);
                Disciplines.ExceptWith(disciplines_);
            }
            return Disciplines.ToArray();
        }
        void RemoveTeachersFromGroup(IEnumerable<string> teachers, string groupName)
        {
            foreach (var teachertLogin in teachers)
            {
                Teacher teacher = settings.LoadAccount(teachertLogin) as Teacher;
                if (teacher == null) continue;
                teacher.Groups = teacher.Groups.Except(new string[] { groupName }).ToArray();
                teacher.Disciplines = teacher.Disciplines.Except(
                    ListDisciplinesToRemove(teacher, groupName, teacher.Disciplines) // Просто удаляю все ненайденные дисциплины
                    ).ToArray();
                using (var writer = settings.GetAccountFileWriter(teacher.Login))
                    teacher.Export(writer);
            }
        }

        Student GetStudentAndCheckGroup(string studentLogin, out string msg)
        {
            if (!accountsList.Contains(studentLogin))
            {
                msg = $"Студента с логином {studentLogin} не существует";
                return null;
            }
            var account = settings.LoadAccount(studentLogin);
            if (account.Type != AccountType.Student)
            {
                msg = $"Аккаунт с логином {studentLogin} ({(account.Type == AccountType.Teacher ? "преподаватель" : "админимтратор")}) не является студентом";
                return null;
            }
            Student student = account as Student;
            if (student.Group != "")
            {
                msg = $"Студент {studentLogin} ({UIFunctions.GetInitials(student)}) уже состоит в группе {student.Group}";
                return null;
            }
            msg = "";
            return student;
        }
        Teacher GetTeacherAndCheck(string teacherLogin, out string msg)
        {
            if (!accountsList.Contains(teacherLogin))
            {
                msg = $"Преподавателя с логином {teacherLogin} не существует";
                return null;
            }
            var account = settings.LoadAccount(teacherLogin);
            if (account.Type != AccountType.Teacher)
            {
                msg = $"Аккаунт с логином {teacherLogin} ({(account.Type == AccountType.Student ? "студент" : "админимтратор")}) не является преподавателем";
                return null;
            }
            msg = "";
            return account as Teacher;
        }

        void ShowGroupAddMenu()
        {
            UpdateAccountsList();
            menu.Clear();
            Group group = new Group("");
            ClassEditor<Group> editor = new ClassEditor<Group>(
                group,
                (Group group, string[] changedProperties, out string msg) =>
                {
                    if (!validateGroupName(group, changedProperties, out msg)) return false;

                    List<Student> students = new();
                    for (int i = 0; i < group.Students.Count; i++)
                    {
                        string studentLogin = group.Students[i];
                        Student student = GetStudentAndCheckGroup(studentLogin, out msg);
                        if (student == null) return false;
                        students.Add(student);
                    }
                    List<(Teacher teacher, byte[] disciplines)> teachers = new();
                    for (int i = 0; i < group.Teachers.Count; i++)
                    {
                        string teacherLogin = group.Teachers[i];
                        Teacher teacher = GetTeacherAndCheck(teacherLogin, out msg);
                        if (teacher == null) return false;
                        teachers.Add((teacher, group.Teacher_disciplines[(byte)i].ToArray()));
                    }

                    SetStudentsGroup(students, group.Name);
                    SetTeachersGroupAndDisciplines(teachers, group);

                    msg = ""; return true;
                },
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
            UpdateAccountsList();
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
                if (selectedIndex >= 0 && selectedIndex <= 1)
                {
                    menu.Clear();
                    Group group = new Group(settings, groupsList[index]);
                    string _name = group.Name;
                    string[] _students = group.Students.ToArray();
                    string[] _teachers = group.Teachers.ToArray();
                    byte[][] _teachersDisciplines = new byte[group.Teachers.Count][];
                    foreach (var kv in group.Teacher_disciplines)
                        _teachersDisciplines[kv.Key] = kv.Value.ToArray();

                    switch (selectedIndex)
                    {
                        case 0: //Info
                            {
                                ClassEditor<Group> editor = new ClassEditor<Group>(
                                    group,
                                    (Group group, string[] changedProperties, out string msg) =>
                                    {
                                        if (!validateGroupName(group, changedProperties, out msg)) return false;

                                        List<Student> newStudentsList = new();
                                        var newStudents = group.Students.Except(_students);
                                        var deletedStudents = _students.Except(group.Students);

                                        foreach (string studentLogin in newStudents)
                                        {
                                            Student student = GetStudentAndCheckGroup(studentLogin, out msg);
                                            if (student == null) return false;
                                            newStudentsList.Add(student);
                                        }

                                        List<(Teacher teacher, byte[] disciplines)> newTeachersList = new();
                                        List<(Teacher teacher, byte[] _disciplines, byte[] disciplines)> updatedTeachersList = new();
                                        var newTeachers = group.Teachers.Except(_teachers);
                                        var deletedTeachers = _teachers.Except(group.Teachers);
                                        for (int i = 0; i < group.Teachers.Count; i++)
                                        {
                                            string teacherLogin = group.Teachers[i];
                                            if (newTeachers.Contains(teacherLogin))
                                            {
                                                Teacher teacher = GetTeacherAndCheck(teacherLogin, out msg);
                                                if (teacher == null) return false;
                                                newTeachersList.Add((teacher, group.Teacher_disciplines[(byte)i].ToArray()));
                                            }
                                            else
                                            {
                                                List<byte> disciplinesList = group.Teacher_disciplines[(byte)i];
                                                byte[] _disciplinesList = _teachersDisciplines[Array.IndexOf(_teachers, teacherLogin)];
                                                if (disciplinesList.SequenceEqual(_disciplinesList)) continue;
                                                Teacher teacher = settings.LoadAccount(teacherLogin) as Teacher;
                                                updatedTeachersList.Add((teacher, _disciplinesList, disciplinesList.ToArray()));
                                            }
                                        }
                                        foreach (string teacherLogin in deletedTeachers)
                                        {
                                            Teacher teacher = settings.LoadAccount(teacherLogin) as Teacher;
                                            byte[] _disciplinesList = _teachersDisciplines[Array.IndexOf(_teachers, teacherLogin)];

                                        }

                                        SetStudentsGroup(deletedStudents, "");
                                        SetStudentsGroup(newStudentsList, group.Name);
                                        SetTeachersGroupAndDisciplines(newTeachersList, group);
                                        UpdateTeachersGroupAndDisciplines(updatedTeachersList, group, _name != group.Name ? group.Name : null);
                                        RemoveTeachersFromGroup(deletedTeachers, group.Name);

                                        msg = ""; return true;
                                    },
                                    new ICustomEditor<Group>[]
                                    {
                                        new TeachersAndDisciplinesEditor(group, "Дисциплины преподавателей")
                                    }
                                );
                                if (editor.Edit())
                                    group.ExportInfo(settings);
                                editor.Clear();
                            }
                            break;
                        case 1: //Journal
                            {
                                JournalEditor editor = new JournalEditor(group, CurrentAccount, settings);
                                if (editor.Edit())
                                    group.ExportJournal(settings);
                                //editor.Clear();
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