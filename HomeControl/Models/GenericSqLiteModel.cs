namespace HomeControl.Models
{
    public abstract class GenericSqLiteModel<T> : SqLiteModel where T : SqLiteModel
    {
        public static T Select()
        {
            return Select<T>(null);
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