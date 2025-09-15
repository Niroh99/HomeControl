using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NTIH.Database.Modeling;

namespace HomeControl.Models.Modeling
{
    public static class Display
    {
        public static void RegisterDisplayTypesFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            foreach (var displayType in assembly.DefinedTypes.Where(x => x.IsAssignableTo(typeof(IDisplay<>))))
            {
                services.AddTransient(displayType);
            }
        }

        public static async Task<IDisplay> CreateDisplayAsync(this IDisplayable displayable, IServiceProvider serviceProvider)
        {
            var displayableType = displayable.GetType();

            var genericDisplayType = typeof(IDisplay<>).MakeGenericType(displayableType);

            var display = (IDisplay)serviceProvider.GetService(genericDisplayType);

            await display.Create(displayable, serviceProvider);

            return display;
        }
    }
}