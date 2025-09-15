using HomeControl.CLI.Attributes;
using NTIH.Database;
using NTIH.Database.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace HomeControl.CLI.CommandHandlers
{
    [CommandHandler]
    public class AddCommandHandler(Queue<string> args) : ModelCommandHandler(args)
    {
        protected override string[] Options => [];

        public override async Task<int> HandleAsync()
        {
            var baseExitCode = await base.HandleAsync();

            if (baseExitCode != 0) return baseExitCode;

            if (Args.Count == 0)
            {
                Console.WriteLine("No model name supplied.");
                return 1;
            }

            var modelName = Args.Dequeue();

            if (string.IsNullOrWhiteSpace(modelName))
            {
                Console.WriteLine("No model name supplied.");
                return 1;
            }

            if (Args.Count % 2 != 0)
            {
                Console.WriteLine("Invalid number of arguments. Each member should be followed by its value.");
                return 1;
            }

            DatabaseConnection.RegisterDatabaseModelTypesFromAssembly(Models.AssemblyReference.Value);

            if (!DatabaseConnection.TryGetTableModelMetadata(modelName, out var modelType, out var metadata))
            {
                Console.WriteLine($"Unknown modelType {modelName}.");
                return 1;
            }

            var instance = Activator.CreateInstance(modelType);

            do
            {
                var member = Args.Dequeue();
                var value = Args.Dequeue();

                var field = metadata.Fields.OfType<DatabaseColumnField>().FirstOrDefault(field => string.Compare(field.Name, member, StringComparison.OrdinalIgnoreCase) == 0);

                if (field == null)
                {
                    Console.WriteLine($"Unknown member {member}.");
                    return 1;
                }

                try
                {
                    if (field.IsJson)
                    {
                        var fieldValue = System.Text.Json.JsonSerializer.Deserialize(value, field.PropertyInfo.PropertyType);
                    }

                    var convertedValue = Convert.ChangeType(value, field.PropertyInfo.PropertyType);

                    field.PropertyInfo.SetValue(instance, convertedValue);
                }
                catch
                {
                    Console.WriteLine($"Invalid value for member {field.Name}: {value}");
                }
            }
            while (Args.Count > 0);

            try
            {
                await HostHttpClient.PutAsync($"{ModelControllerAddress}/{modelName}", JsonContent.Create(instance));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }

            return 0;
        }
    }
}