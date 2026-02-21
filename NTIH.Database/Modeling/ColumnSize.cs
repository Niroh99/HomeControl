using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NTIH.Database.Modeling
{
    public readonly struct ColumnSize(int precision, int scale)
    {
        public ColumnSize(int precision) : this(precision, 0)
        { }

        public int Precision { get; } = precision;

        public int Scale { get; } = scale;

        public static implicit operator ColumnSize(int precision)
        {
            return new ColumnSize(precision);
        }
    }
}
