namespace HomeControl.Views.Shared
{
    public class BreadcrumbModel
    {
        public BreadcrumbModel(string id, string separator, params Breadcrumb[] breadcrumbs) : this(id, separator, [], breadcrumbs)
        {

        }

        public BreadcrumbModel(string id, string separator, List<string> separatorClasses, params Breadcrumb[] breadcrumbs)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);
            ArgumentException.ThrowIfNullOrWhiteSpace(separator);

            Id = id;
            Separator = separator;
            SeparatorClasses.AddRange(separatorClasses);
            Breadcrumbs.AddRange(breadcrumbs);
        }

        public string Id { get; }

        public List<Breadcrumb> Breadcrumbs { get; } = [];

        public string Separator { get; set; }

        public List<string> SeparatorClasses { get; } = [];
    }

    public class Breadcrumb
    {
        public Breadcrumb(string name, string uri, params string[] classes)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            Name = name;
            Uri = uri;
            Classes.AddRange(classes);
        }

        public string Name { get; set; }

        public string Uri { get; set; }

        public bool IsEnabled { get => Uri != null; }

        public List<string> Classes { get; } = [];
    }
}