using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HomeControl.Models.Modeling
{
    public interface IDisplayFactory
    {
        Task<IDisplay> CreateDisplayAsync(IDisplayable displayable);
    }

    public class DisplayFactory(IServiceProvider serviceProvider) : IDisplayFactory
    {
        private static readonly Dictionary<Type, Type> _displayTypeMap = [];

        public static void RegisterDisplaysFromAssembly(Assembly assembly)
        {
            foreach (var displayType in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(typeof(IDisplay))))
            {
                var interfaces = displayType.GetInterfaces();

                var genericDisplayType = interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDisplay<>));

                if (genericDisplayType == null)
                    continue;

                var displayableType = genericDisplayType.GetGenericArguments()[0];

                _displayTypeMap[displayableType] = displayType;
            }
        }

        public static void RegisterDisplay<TDisplayable, TDisplay>() where TDisplayable : IDisplayable where TDisplay : IDisplay<TDisplayable>
        {
            _displayTypeMap[typeof(IDisplayable)] = typeof(IDisplay);
        }

        public async Task<IDisplay> CreateDisplayAsync(IDisplayable displayable)
        {
            var displayableType = displayable.GetType();

            Type displayType;

            while (!_displayTypeMap.TryGetValue(displayableType, out displayType))
            {
                if (displayableType.BaseType == null)
                    throw new InvalidOperationException($"No display registered for type {displayable.GetType().FullName}");

                displayableType = displayableType.BaseType;
            }

            var display = (IDisplay)Activator.CreateInstance(displayType);

            await display.Create(displayable, serviceProvider);

            return display;
        }
    }
}