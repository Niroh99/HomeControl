using HomeInventory.Integrations.HomeControl;
using HomeInventory.Models;
using HomeInventory.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HomeInventory.ViewModels
{
    public class StockViewModel : BaseViewModel
    {
        public StockViewModel()
        {
            Title = "Stock";
        }

        private readonly IHomeControlInventoryService _homeControlInventoryService = DependencyService.Get<IHomeControlInventoryService>();

        public ObservableCollection<Stock> Stock { get; } = new ObservableCollection<Stock>();

        public Command RefreshCommand { get => new Command(async () => await RefreshAsync()); }

        private Stock _selectedItem;
        public Stock SelectedItem
        {
            get { return _selectedItem; }
            set { if (SetProperty(ref _selectedItem, value)) OnSelectedItemChanged(); }
        }

        private async Task RefreshAsync()
        {
            await GetStockAsync();
        }

        protected override async Task OnViewAppearingAsync()
        {
            await GetStockAsync();
        }

        private async Task GetStockAsync()
        {
            await Busy(async () =>
            {
                try
                {
                    var stock = await _homeControlInventoryService.Get();

                    Stock.Clear();

                    foreach (var stockItem in stock)
                    {
                        Stock.Add(stockItem);
                    }
                }
                catch (Exception ex)
                {
                    ToastMessage.LongAlert(ex.Message);
                }
            });
        }

        private async void OnSelectedItemChanged()
        {
            if (SelectedItem == null) return;

            await Shell.Current.GoToAsync($"{nameof(StockItemPage)}?{nameof(StockItemViewModel.StockItemId)}={SelectedItem.Id}");
            
            SelectedItem = null;
        }
    }
}
