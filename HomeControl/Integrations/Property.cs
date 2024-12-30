namespace HomeControl.Integrations
{
    public class Property : IProperty
    {
        public Property(string label)
        {
            Label = label;
        }

        public string Label { get; }

        public bool IsPlaceholder => false;
    }
}