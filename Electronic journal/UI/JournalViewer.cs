using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electronic_journal
{
    public class JournalViewer
    {
        public Group Group_;
        public Student Student_;

        public JournalViewer(Group group, Student student)
        {
            Group_ = group;
            Student_ = student;
        }
    }
}
