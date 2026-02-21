using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeControl.Models.Modeling
{
    public abstract class DisplayBase<T> : IDisplay<T> where T : IDisplayable
    {
        public string Display { get; protected set; }

        public string AdditionalInfo { get; protected set; }

        public abstract Task Create(T displayable, IServiceProvider serviceProvider);

        Task IDisplay.Create(object displayable, IServiceProvider serviceProvider)
        {
            return Create((T)displayable, serviceProvider);
        }
    }
}