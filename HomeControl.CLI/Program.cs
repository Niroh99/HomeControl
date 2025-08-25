using HomeControl.CLI.CommandHandlers;
using HomeControl.CLI.Helpers;

Queue<string> arguments;

if (System.Diagnostics.Debugger.IsAttached)
{
    while (true)
    {
        arguments = DebugHelper.ReadConsoleArguments();
        var exitCode = await ProcessCommand(arguments);
        Console.WriteLine($"Exit code: {exitCode}");
    }
}
else
{
    arguments = new Queue<string>(args);
    var exitCode = await ProcessCommand(arguments);
    Environment.Exit(exitCode);
}

static async Task<int> ProcessCommand(Queue<string> arguments)
{
    if (arguments.Count == 0)
    {
        Console.WriteLine("No command provided.");
        PrintAvailableCommands();
        return 1;
    }

    var command = arguments.Dequeue().ToLowerInvariant();

    var commandHandlerTypes = CommandHandler.GetCommandHandlerTypes();

    if (commandHandlerTypes.TryGetValue(command, out var commandHandlerType))
    {
        var commandHandler = (CommandHandler)Activator.CreateInstance(commandHandlerType, arguments)!;
        var exitCode = await commandHandler.HandleAsync();

        if (commandHandler is IDisposable disposableCommandHandler)
        {
            disposableCommandHandler.Dispose();
        }

        return exitCode;
    }
    else
    {
        Console.WriteLine($"Unknown command: {command}");
        PrintAvailableCommands();
        return 1;
    }
}

static void PrintAvailableCommands()
{
    Console.WriteLine("Available commands:");
    foreach (var availableCommand in CommandHandler.GetCommandHandlerTypes().Keys)
    {
        Console.WriteLine($"- {availableCommand}");
    }
}