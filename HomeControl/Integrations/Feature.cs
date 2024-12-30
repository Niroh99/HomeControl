namespace HomeControl.Integrations
{
    public class Feature
    {
        public Feature(string name, Action execute)
        {
            Name = name;
            Execute = execute;
        }

        public string Name { get; }

        public Action Execute { get; }
    }
}
