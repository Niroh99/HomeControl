using HomeInventory.Services;
using HomeInventory.ViewModels;
using HomeInventory.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace HomeInventory
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ProductPage), typeof(ProductPage));
            Routing.RegisterRoute(nameof(StockItemPage), typeof(StockItemPage));

            if (!_settingsService.IsValid) Navigation.PushAsync(new SettingsPage());

            if (System.Diagnostics.Debugger.IsAttached)
            {
                var testFlyoutItem = new FlyoutItem { Title = "Test" };

                testFlyoutItem.Items.Add(new ShellContent { Content = new TestPage() });

                Items.Add(testFlyoutItem);
            }
        }

        private readonly ISettingsService _settingsService = DependencyService.Get<ISettingsService>();
    }
}
