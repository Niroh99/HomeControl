using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeControl.CLI.CommandHandlers
{
    public abstract class ModelCommandHandler(Queue<string> args) : HostCommandHandler(args)
    {
        protected const string ModelControllerAddress = "Data";
    }
}