using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeControl.CLI.CommandHandlers
{
    public abstract class HostCommandHandler(Queue<string> args) : CommandHandler(args)
    {
        public override async Task<int> HandleAsync()
        {
            var config = Config.Load();

            if (string.IsNullOrWhiteSpace(config.HostUrl))
            {
                Console.WriteLine($"{nameof(Config.HostUrl)} has not been configured.");
                return 1;
            }

            HostHttpClient = new HttpClient()
            {
                BaseAddress = new Uri(config.HostUrl)
            };

            await Task.CompletedTask;
            return 0;
        }

        protected HttpClient HostHttpClient { get; private set; }
    }
}
