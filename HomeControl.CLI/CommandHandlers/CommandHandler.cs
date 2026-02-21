using HomeControl.CLI.Attributes;
using NTIH.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HomeControl.CLI.CommandHandlers
{
    public abstract class CommandHandler(Queue<string> args)
    {
        private readonly Queue<string> _args = args;
        protected Queue<string> Args => _args;

        protected abstract string[] Options { get; }

        public static Dictionary<string, Type> GetCommandHandlerTypes()
        {
            var commandHandlerTypes = new Dictionary<string, Type>();

            var assembly = Assembly.GetExecutingAssembly();

            foreach (var commandHandlerType in assembly.DefinedTypes.Where(x => x.IsAssignableTo(typeof(CommandHandler))))
            {
                if (!commandHandlerType.TryGetCustomAttribute<CommandHandlerAttribute>(out var commandHandlerAttribute)) continue;

                var commandName = commandHandlerAttribute.Command;

                if (commandName == null)
                {
                    commandName = commandHandlerType.Name;

                    if (commandName.EndsWith("CommandHandler", StringComparison.OrdinalIgnoreCase))
                    {
                        commandName = commandName[..^"CommandHandler".Length];
                    }

                    if (string.IsNullOrEmpty(commandName)) continue;
                }

                commandHandlerTypes[commandName.ToLowerInvariant()] = commandHandlerType.AsType();
            }

            return commandHandlerTypes;
        }

        public abstract Task<int> HandleAsync();

        protected IEnumerable<string> GetCurrentCommandValues()
        {
            while (Args.Count > 0 && !Options.Contains(Args.Peek()))
            {
                yield return Args.Dequeue();
            }
        }
    }
}