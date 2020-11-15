using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Electronic_journal
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class EditorAttribute : Attribute
    {
        public EditorAttribute(string displayValue)
        {
            DisplayValue = displayValue;
        }

        public string DisplayValue { get; set; }
    }
    
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class DateTimeParamsAttribute : Attribute
    {
        public DateTimeParamsAttribute() { }
        public bool OnlyDate { get; set; }
    }
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class StringParamsAttribute : Attribute
    {
        public StringParamsAttribute() { }
        public bool AllowEmpty { get; set; }
    }
}
