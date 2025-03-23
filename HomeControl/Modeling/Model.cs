namespace HomeControl.Modeling
{
    public abstract class Model
    {
        private readonly Dictionary<string, object> _properties = [];
        private readonly Dictionary<string, object> _modifiedProperties = [];

        public T Get<T>([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (TryGetModifiedPropertyValue<T>(propertyName, out var modifiedValue)) return modifiedValue;

            return GetPropertyValue<T>(propertyName);
        }

        public void Set<T>(T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            SetCore(value, propertyName);
        }

        public void ApplyChanges()
        {
            foreach (var property in _modifiedProperties) _properties[property.Key] = property.Value;

            _modifiedProperties.Clear();
        }

        public void Reset()
        {
            _modifiedProperties.Clear();
        }

        public string[] GetModifiedProperties()
        {
            return _modifiedProperties.Keys.ToArray();
        }

        private T GetPropertyValue<T>(string propertyName)
        {
            if (_properties.TryGetValue(propertyName, out var value)) return (T)value;

            return default;
        }

        private bool TryGetModifiedPropertyValue<T>(string propertyName, out T modifiedValue)
        {
            if (_modifiedProperties.TryGetValue(propertyName, out var modifiedPropertyValue))
            {
                modifiedValue = (T)modifiedPropertyValue;
                return true;
            }

            modifiedValue = default;
            return false;
        }

        private void SetCore(object value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            _modifiedProperties[propertyName] = value;
        }
    }
}