using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace NTIH.ViewModeling
{
    public static class DIExtensions
    {
        public static void RegisterViewModelsFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            foreach (var type in assembly.GetTypes().Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(ViewModel))))
            {
                services.AddTransient(type);
            }
        }
    }
}
