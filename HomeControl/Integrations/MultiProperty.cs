namespace HomeControl.Integrations
{
    public class MultiProperty : Property
    {
        public MultiProperty(string label, List<IProperty> childProperties) : base(label)
        {
            ChildProperties = childProperties;
        }

        public List<IProperty> ChildProperties { get; }
    }
}