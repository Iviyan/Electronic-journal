using System;
using System.Collections.Generic;
using System.Text;

namespace Electronic_journal
{
    public sealed class EditorAttribute : Attribute
    {
        public EditorAttribute(string displayValue)
        {
            DisplayValue = displayValue;
        }

        public string DisplayValue { get; set; }
    }
}
