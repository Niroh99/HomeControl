using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HomeControl.Models
{
    public static class AssemblyReference
    {
        public static Assembly Value { get => typeof(AssemblyReference).Assembly; }
    }
}
