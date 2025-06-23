using System.Collections;

namespace HomeControl.Modeling
{
    public class CollectionModel<T> : Model, ICollection<T>
    {
        private readonly List<T> _items = [];

        public int Count => _items.Count;

        public bool IsReadOnly => false;

        public void Add(T item) => _items.Add(item);

        public void AddRange(IEnumerable<T> Collection) => _items.AddRange(Collection);

        public void Clear() => _items.Clear();

        public bool Contains(T item) => _items.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        public bool Remove(T item) => _items.Remove(item);

        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
    }
}