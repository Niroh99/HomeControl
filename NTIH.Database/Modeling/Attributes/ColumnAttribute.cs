using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTIH.Database.Modeling.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute()
        { }

        public ColumnAttribute(ColumnSize columnSize)
        {
            Size = columnSize;
        }

        public ColumnAttribute(int precision, int scale)
        {
            Size = new ColumnSize(precision, scale);
        }

        public string Name { get; set; }

        public ColumnSize Size { get; set; }
    }
}
