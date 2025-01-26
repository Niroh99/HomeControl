namespace HomeControl.Models
{
    public abstract class Model
    {
        protected Dictionary<string, object> _properties = new Dictionary<string, object>();

        public T Get<T>([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (_properties.TryGetValue(propertyName, out object value))
            {
                var type = typeof(T);

                if (type.IsEnum && value is string stringValue)
                {
                    value = Enum.Parse(type, stringValue);

                    _properties[propertyName] = value;
                }
                else if (type == typeof(int) && value is long longValue)
                {
                    value = (int)longValue;

                    _properties[propertyName] = value;
                }

                return (T)value;
            }

            return default;
        }

        public void Set<T>(T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            SetCore(value, propertyName);
        }

        protected void SetCore(object value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            _properties[propertyName] = value;
        }
    }
}