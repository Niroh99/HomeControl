using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeControl.CLI.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandHandlerAttribute(string? command) : Attribute
    {
        public CommandHandlerAttribute() : this(null) { }

        public string? Command { get; } = command;
    }
}