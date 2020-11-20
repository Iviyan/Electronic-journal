using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electronic_journal
{
    public class Group
    {
        [Editor("Имя"), StringParams(AllowEmpty = false)]
        public string Name { get; set; }
        [Editor("Дисциплины")]
        public ObservableCollection<string> Disciplines { get; set; }
        [Editor("Преподаватели")]
        public ObservableCollection<string> Teachers { get; set; }
        [Editor("Студенты")]
        public ObservableCollection<string> Students { get; set; }

        /// <summary>
        /// key - teacherID<br/>
        /// value - disciplineID[]
        /// </summary>
        public Dictionary<byte, List<byte>> Teacher_disciplines;

        public List<List<List<(byte mark, DateTime date)>>> Marks;

        public Group(string groupName)
        {
            Name = groupName;

            Disciplines = new();
            Teachers = new();
            Teacher_disciplines = new();
            Students = new();
            Marks = new();

            Students.CollectionChanged += Students_CollectionChanged;
            Disciplines.CollectionChanged += Disciplines_CollectionChanged;
            Teachers.CollectionChanged += Teachers_CollectionChanged;
        }
        public Group(Settings settings, string groupName) : this(groupName, settings.GetGroupInfoFilePatch(groupName), settings.GetGroupJournalFilePatch(groupName)) { }
        public Group(string groupName, string groupFile_info, string groupFile_journal)
        {
            if (Name == null) Name = groupName;
            Disciplines = new();
            Teachers = new();
            Teacher_disciplines = new();
            Students = new();
            Marks = new();

            using (BinaryReader reader = new BinaryReader(File.Open(groupFile_info, FileMode.Open)))
            {
                byte disciplinesCount = reader.ReadByte();
                for (byte i = 0; i < disciplinesCount; i++)
                    Disciplines.Add(reader.ReadString());

                byte teachersCount = reader.ReadByte();
                for (byte i = 0; i < teachersCount; i++)
                {
                    Teachers.Add(reader.ReadString());
                    byte teacherDisciplinesCount = reader.ReadByte();
                    List<byte> teacherDisciplines = new(teacherDisciplinesCount);
                    for (byte j = 0; j < teacherDisciplinesCount; j++)
                        teacherDisciplines.Add(reader.ReadByte());
                    Teacher_disciplines.Add(i, teacherDisciplines);
                }

                byte studentsCount = reader.ReadByte();
                for (int i = 0; i < studentsCount; i++)
                    Students.Add(reader.ReadString());
            }

            using (BinaryReader reader = new BinaryReader(File.Open(groupFile_journal, FileMode.Open)))
            {
                for (byte i = 0; i < Students.Count; i++)
                {
                    Marks.Add(new());
                    var studentMarks = Marks[i];
                    for (byte j = 0; j < Disciplines.Count; j++)
                    {
                        byte markCount = reader.ReadByte();
                        studentMarks.Add(new(markCount));
                        var disciplineMarks = studentMarks[j];

                        for (byte k = 0; k < markCount; k++)
                            disciplineMarks.Add((reader.ReadByte(), reader.ReadDateTime()));

                        studentMarks.Add(disciplineMarks);
                    }
                }
            }

            Students.CollectionChanged += Students_CollectionChanged;
            Disciplines.CollectionChanged += Disciplines_CollectionChanged;
            Teachers.CollectionChanged += Teachers_CollectionChanged;
        }

        private void Students_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Marks.Insert(e.NewStartingIndex, new());
                    var studentMarks = Marks[e.NewStartingIndex];
                    for (int i = 0; i < Disciplines.Count; i++)
                        studentMarks.Add(new());
                    //Helper.mb("sadd ", e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Marks.RemoveAt(e.OldStartingIndex);
                    //Helper.mb("srem ", e.OldStartingIndex);
                    break;
            }
        }
        private void Disciplines_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < Marks.Count; i++)
                        Marks[i].Insert(e.NewStartingIndex, new());

                    if (e.NewStartingIndex + 1 < Disciplines.Count)
                    {
                        for (byte i = 0; i < Teachers.Count - 1; i++)
                            for (byte j = 0; i < Teacher_disciplines[i].Count; i++)
                                if (Teacher_disciplines[i][j] >= e.NewStartingIndex)
                                    Teacher_disciplines[i][j]++;

                    }
                    //Helper.mb("dadd ", e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < Marks.Count; i++)
                        Marks[i].RemoveAt(e.OldStartingIndex);

                    for (byte i = 0; i < Teachers.Count - 1; i++)
                    {
                        var disciplinesList = Teacher_disciplines[i];
                        disciplinesList.Remove((byte)e.OldStartingIndex);
                        for (int j = 0; j < disciplinesList.Count; j++)
                            if (disciplinesList[j] > e.OldStartingIndex)
                                disciplinesList[j]--;
                    }
                    //Helper.mb("drem ", e.OldStartingIndex);
                    break;
            }
        }
        private void Teachers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex + 1 == Teachers.Count)
                        Teacher_disciplines.Add((byte)e.NewStartingIndex, new());
                    else
                    {
                        for (byte i = (byte)e.NewStartingIndex; i < Teachers.Count - 1; i++)
                            Teacher_disciplines.ChangeKey(i, (byte)(i + 1));
                        Teacher_disciplines.Add((byte)e.NewStartingIndex, new());
                    }
                    //Helper.mb("tadd ", e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex == Teachers.Count)
                        Teacher_disciplines.Remove((byte)e.NewStartingIndex);
                    else
                    {
                        Teacher_disciplines.Remove((byte)e.NewStartingIndex);
                        for (byte i = (byte)(e.NewStartingIndex + 1); i < Teachers.Count - 1; i++)
                            Teacher_disciplines.ChangeKey(i, (byte)(i - 1));
                    }
                    //Helper.mb("trem ", e.OldStartingIndex);
                    break;
                    
            }
        }

        public void AddDisciplineForTeacher(byte teacherIndex, byte disciplineIndex)
        {
            if (!Teacher_disciplines[teacherIndex].Contains(disciplineIndex))
                Teacher_disciplines[teacherIndex].Add(disciplineIndex);
        }
        public void SetDisciplinesForTeacher(byte teacherIndex, byte[] disciplineIndexes) => Teacher_disciplines[teacherIndex] = disciplineIndexes.ToList();
        public void RemoveDisciplineFromTeacher(byte teacherIndex, byte disciplineIndex)
        {
            if (Teacher_disciplines[teacherIndex].Contains(disciplineIndex))
                Teacher_disciplines[teacherIndex].Remove(disciplineIndex);
        }

        public List<(byte mark, DateTime date)> GetMarksList(byte studentIndex, byte disciplineIndex) => Marks[studentIndex][disciplineIndex];
        public void AddMark(byte studentIndex, byte disciplineIndex, byte mark, DateTime date)
        {
            Marks[studentIndex][disciplineIndex].Add((mark, date));
        }

        public void Export(Settings settings)
        {
            ExportInfo(settings);
            ExportJournal(settings);
        }
        public void ExportInfo(Settings settings)
        {
            if (!Directory.Exists(settings.GetGroupFolderPatch(Name)))
                Directory.CreateDirectory(settings.GetGroupFolderPatch(Name));

            using (BinaryWriter writer = new BinaryWriter(File.Open(settings.GetGroupInfoFilePatch(Name), FileMode.Create)))
            {
                writer.Write((byte)Disciplines.Count);
                for (byte i = 0; i < Disciplines.Count; i++)
                    writer.Write(Disciplines[i]);

                writer.Write((byte)Teachers.Count);
                for (byte i = 0; i < Teachers.Count; i++)
                {
                    writer.Write(Teachers[i]);
                    writer.Write((byte)Teacher_disciplines[i].Count);
                    foreach (byte d in Teacher_disciplines[i])
                        writer.Write(d);
                }

                writer.Write((byte)Students.Count);
                for (byte i = 0; i < Students.Count; i++)
                    writer.Write(Students[i]);
            }
        }
        public void ExportJournal(Settings settings)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(settings.GetGroupJournalFilePatch(Name), FileMode.Create)))
            {
                for (byte i = 0; i < Students.Count; i++)
                {
                    for (byte j = 0; j < Disciplines.Count; j++)
                    {
                        var disciplineMarks = Marks[i][j];
                        writer.Write((byte)disciplineMarks.Count);

                        foreach (var mark in disciplineMarks)
                        {
                            writer.Write(mark.mark);
                            writer.Write(mark.date);
                        }
                    }
                }
            }
        }
    }
}
