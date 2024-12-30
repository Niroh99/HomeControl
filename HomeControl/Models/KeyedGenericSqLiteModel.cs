namespace HomeControl.Models
{
    public abstract class KeyedGenericSqLiteModel<T, TKey> : SqLiteModel where T : SqLiteModel
    {
        public static T Select(TKey id)
        {
            return Select<T>(id);
        }

        public static List<T> SelectAll()
        {
            return SelectAll<T>();
        }

        public static void Insert(T instance)
        {
            Insert<T>(instance);
        }
    }
}