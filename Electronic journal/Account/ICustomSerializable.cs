using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Electronic_journal
{
    interface ICustomSerializable
    {
        void Export(BinaryWriter writer);
    }
}
