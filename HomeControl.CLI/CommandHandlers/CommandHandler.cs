using HomeControl.CLI.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HomeControl.CLI.CommandHandlers
{
    public abstract class CommandHandler
    {
        public static Dictionary<string, Type> GetCommandHandlerTypes()
        {
            var commandHandlerTypes = new Dictionary<string, Type>();

            var assembly = Assembly.GetExecutingAssembly();

            foreach (var commandHandlerType in assembly.DefinedTypes.Where(x => x.IsAssignableTo(typeof(CommandHandler))))
            {
                if (commandHandlerType.GetCustomAttribute(typeof(CommandHandlerAttribute)) is not CommandHandlerAttribute commandHandlerAttribute) continue;

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

        public abstract Task<int> HandleAsync(Queue<string> args);
    }
}