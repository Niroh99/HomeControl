namespace HomeControl.Integrations
{
    public class SingleProperty : Property
    {
        public SingleProperty(string label, string value) : base(label)
        {
            Value = value;
        }

        public string Value { get; }
    }
}