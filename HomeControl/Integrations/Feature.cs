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

        public Func<Task> Execute { get; }
    }
}
