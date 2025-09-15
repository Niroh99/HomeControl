using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<IDisplay> CreateDisplayAsync(IDisplayable displayable)
        {
            var displayableType = displayable.GetType();

            var genericDisplayableType = displayableType.GetInterfaces().FirstOrDefault(ii => ii.IsAssignableTo(typeof(IDisplayable)) && ii.IsGenericType);

            var displayType = genericDisplayableType.GetGenericArguments()[0];

            var display = (IDisplay)Activator.CreateInstance(displayType);

            await display.Create(displayable, serviceProvider);

            return display;
        }
    }
}