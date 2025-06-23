using HomeInventory.Integrations.HomeControl;
using HomeInventory.Integrations.OpenFoodFacts;
using HomeInventory.Services;
using HomeInventory.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HomeInventory
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<OpenFoodFactsService>();
            DependencyService.Register<HomeControlDatabaseModelService>();
            DependencyService.Register<HomeControlInventoryService>();
            DependencyService.RegisterSingleton(new SettingsService());
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
