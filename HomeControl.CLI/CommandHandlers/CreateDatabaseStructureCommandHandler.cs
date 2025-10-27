using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeControl.CLI.CommandHandlers
{
    [Attributes.CommandHandler]
    public class CreateDatabaseStructureCommandHandler(Queue<string> args) : HostCommandHandler(args)
    {
        private const string ControllerAddress = "database/structure";

        protected override string[] Options => [];

        public override async Task<int> HandleAsync()
        {
            await base.HandleAsync().ConfigureAwait(false);

            try
            {
                var response = await HostHttpClient.PostAsync($"{ControllerAddress}/create", null).ConfigureAwait(false);

                if (response.IsSuccessStatusCode) return 0;
                else
                {
                    Console.WriteLine("Something went wrong.");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }
    }
}
