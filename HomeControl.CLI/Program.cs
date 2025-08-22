using HomeControl.CLI.CommandHandlers;

Queue<string> arguments;

if (System.Diagnostics.Debugger.IsAttached)
{
    var debugArgumentsString = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(debugArgumentsString)) return;

    arguments = new Queue<string>();

    do
    {
        debugArgumentsString = debugArgumentsString.TrimStart();

        if (string.IsNullOrEmpty(debugArgumentsString)) break;

        string argument;

        int argumentLengthOffset = 0;

        if (debugArgumentsString[0] == '"')
        {
            debugArgumentsString = debugArgumentsString[1..];

            var endQuoteIndex = debugArgumentsString.IndexOf('"');

            if (endQuoteIndex >= 0)
            {
                argument = debugArgumentsString[..endQuoteIndex];
                argumentLengthOffset = 1;
            }
            else argument = debugArgumentsString;
        }
        else
        {
            var whitespaceIndex = debugArgumentsString.IndexOf(' ');

            if (whitespaceIndex >= 0) argument = debugArgumentsString[..whitespaceIndex];
            else argument = debugArgumentsString;
        }

        arguments.Enqueue(argument);

        debugArgumentsString = debugArgumentsString[(argument.Length + argumentLengthOffset)..];
    }
    while (!string.IsNullOrEmpty(debugArgumentsString));
}
else
{
    if (args.Length == 0)
    {
        Console.WriteLine("No command provided.");
        PrintAvailableCommands();
        return;
    }

    arguments = new Queue<string>(args);
}

var command = arguments.Dequeue().ToLowerInvariant();

var commandHandlerTypes = CommandHandler.GetCommandHandlerTypes();

if (commandHandlerTypes.TryGetValue(command, out var commandHandlerType))
{
    var commandHandler = (CommandHandler)Activator.CreateInstance(commandHandlerType)!;
    var exitCode = await commandHandler.HandleAsync(arguments);

    Environment.Exit(exitCode);
    return;
}
else
{
    Console.WriteLine($"Unknown command: {command}");
    PrintAvailableCommands();
}

void PrintAvailableCommands()
{
    Console.WriteLine("Available commands:");
    foreach (var availableCommand in CommandHandler.GetCommandHandlerTypes().Keys)
    {
        Console.WriteLine($"- {availableCommand}");
    }
}