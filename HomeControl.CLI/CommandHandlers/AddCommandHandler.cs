using HomeControl.CLI.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeControl.CLI.CommandHandlers
{
    [CommandHandler]
    public class AddCommandHandler : CommandHandler
    {
        public override async Task<int> HandleAsync(Queue<string> args)
        {
            await Task.CompletedTask;
            return 0;
        }
    }
}