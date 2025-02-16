
namespace HomeControl.Models
{
    public class MenuItem
    {
        public MenuItem(string name, string url, params MenuItem[] items)
        {
            Name = name;
            Url = url;
            foreach (var item in items) Add(item);
        }

        private MenuItem _parent;
        public MenuItem Parent { get => _parent; }

        public string Name { get; }

        public string Url { get; }

        private List<MenuItem> items { get; } = [];

        public IReadOnlyList<MenuItem> Items { get => items.AsReadOnly(); }

        public void Add(MenuItem item)
        {
            if (item._parent != null) throw new InvalidOperationException("Item already attached to a Parent.");

            item._parent = this;
            items.Add(item);
        }
    }
}