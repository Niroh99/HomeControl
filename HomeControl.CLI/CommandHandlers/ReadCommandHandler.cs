using HomeControl.CLI.Attributes;
using NTIH.Database;
using System;
using System.Linq;
using System.Text;
using System.Web;

namespace HomeControl.CLI.CommandHandlers
{
    [CommandHandler]
    public class ReadCommandHandler(Queue<string> args) : ModelCommandHandler(args)
    {
        protected override string[] Options => ["-q", "-queue"];

        private static readonly string[] _queryParameterMutations = ["_greater", "_smaller"];

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

            DatabaseConnection.RegisterDatabaseModelTypesFromAssembly(Models.AssemblyReference.Value);

            if (!DatabaseConnection.TryGetMetadata(modelName, out var modelType, out var modelMetadata))
            {
                Console.WriteLine($"Unknown modelType {modelName}.");
                return 1;
            }

            var query = string.Empty;

            while (Args.Count > 0)
            {
                switch (Args.Dequeue().ToLower())
                {
                    case "-q":
                    case "-query":
                        HandleQueryCommand(modelMetadata, out query);
                        break;
                }
            }

            try
            {
                var response = await HostHttpClient.GetAsync($"{ModelControllerAddress}/{modelName}{query}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    var deserializationOptions = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web);

                    var jsonDocument = System.Text.Json.JsonSerializer.Deserialize(jsonResponse, CreateModelListType(modelType), deserializationOptions);

                    var serializationOptions = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.General)
                    {
                        WriteIndented = true
                    };

                    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(jsonDocument, serializationOptions));

                    return 0;
                }

                Console.WriteLine("Connection to host could not be established.");
                Console.WriteLine(response.StatusCode.ToString());
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }

        private Type CreateModelListType(Type modelType)
        {
            var listType = typeof(List<>);

            return listType.MakeGenericType(modelType);
        }

        private int HandleQueryCommand(NTIH.Database.Metadata.DatabaseModelMetadata modelMetadata, out string query)
        {
            query = null;

            var values = new Queue<string>(GetCurrentCommandValues());

            if (values.Count % 2 != 0)
            {
                Console.WriteLine("Invalid number of arguments. Each query member should be followed by its value.");
                return 1;
            }

            var queryStringBuilder = new StringBuilder("?");

            var first = true;

            do
            {
                if (first) first = false;
                else queryStringBuilder.Append('&');

                var name = values.Dequeue();
                var value = values.Dequeue();

                var field = modelMetadata.Fields.FirstOrDefault(field =>
                {
                    if (string.Compare(field.Name, name, StringComparison.OrdinalIgnoreCase) == 0) return true;
                    
                    foreach (var queryParameterMutation in _queryParameterMutations)
                    {
                        if (string.Compare(field.Name + queryParameterMutation, name, StringComparison.OrdinalIgnoreCase) == 0) return true;
                    }

                    return false;
                });

                if (field == null)
                {
                    Console.WriteLine($"Unknown field {name}.");
                    return 1;
                }

                queryStringBuilder.Append(HttpUtility.UrlEncode(name))
                    .Append('=')
                    .Append(HttpUtility.UrlEncode(value));
            }
            while (values.Count > 0);

            query = queryStringBuilder.ToString();
            return 0;
        }
    }
}