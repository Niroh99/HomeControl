using HomeInventory.Integrations.HomeControl;
using HomeInventory.Models;
using HomeInventory.Services;
using HomeInventory.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HomeInventory.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel()
        {
            Title = "Settings";

            BackButtonCommand = new Command((_) => { }, (parameter) => SettingsService.IsValid);
        }

        public ISettingsService SettingsService { get; } = DependencyService.Get<ISettingsService>();

        public Command BackButtonCommand { get; }
    }
}
