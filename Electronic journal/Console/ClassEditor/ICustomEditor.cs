using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electronic_journal
{
    public interface ICustomEditor<T>
    {
        T Obj { get; set; }
        string PropertyName { get; set; }
        string GetStringValue();

        bool Edit(int startY);
        void Clear();

    }
}
