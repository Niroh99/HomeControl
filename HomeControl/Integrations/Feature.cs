using System.Text.Json.Serialization;

namespace HomeControl.Integrations
{
    public class Feature
    {
        public Feature(string name, Func<Task> execute)
        {
            Name = name;
            Execute = execute;
        }

        public string Name { get; }

        [JsonIgnore]
        public Func<Task> Execute { get; }
    }
}
