using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeControl.CLI.Helpers
{
    public static class DebugHelper
    {
        public static Queue<string> ReadConsoleArguments()
        {
            var debugArgumentsString = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(debugArgumentsString)) return [];

            var arguments = new Queue<string>();

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

            return arguments;
        }
    }
}