using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTIH.Database.Modeling.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SizeAttribute(int scale, int precision) : Attribute
    {
        public SizeAttribute(int precision) : this(0, precision)
        {
        }

        public int Scale { get; } = scale;

        public int Precision { get; } = precision;
    }
}
