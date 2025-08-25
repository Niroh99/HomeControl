using HomeControl.CLI.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeControl.CLI.CommandHandlers
{
    [CommandHandler]
    public class ConfigCommandHandler(Queue<string> args) : CommandHandler(args)
    {
        protected override string[] Options => [];

        public override async Task<int> HandleAsync()
        {
            if (Args.Count == 0)
            {
                Console.WriteLine("No member specified. Available members: hosturl");
                return 1;
            }

            if (Args.Count % 2 != 0)
            {
                Console.WriteLine("Invalid number of arguments. Each config member should be followed by its value.");
                return 1;
            }

            var config = Config.Load();

            while (Args.Count > 0)
            {
                var member = Args.Dequeue().ToLowerInvariant();
                var value = Args.Dequeue();

                switch (member)
                {
                    case nameof(Config.HostUrl):
                        config.HostUrl = value;
                        Console.WriteLine($"Host URL set to: {value}");
                        break;
                    default:
                        Console.WriteLine($"Unknown member: {member}.");
                        return 1;
                }
            }

            config.Save();

            await Task.CompletedTask;
            return 0;
        }
    }
}
