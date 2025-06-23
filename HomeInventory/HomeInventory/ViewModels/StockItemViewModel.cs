using HomeInventory.Integrations.HomeControl;
using HomeInventory.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HomeInventory.ViewModels
{
    [QueryProperty(nameof(StockItemId), nameof(StockItemId))]
    public class StockItemViewModel : BaseViewModel
    {
        private readonly IHomeControlInventoryService _homeControlInventoryService = DependencyService.Get<IHomeControlInventoryService>();

        private int _stockItemId;
        public int StockItemId
        {
            get { return _stockItemId; }
            set { if (SetProperty(ref _stockItemId, value)) GetStockItem(); }
        }

        private Stock _stockItem;
        public Stock StockItem
        {
            get { return _stockItem; }
            private set { if (SetProperty(ref _stockItem, value)) OnPropertyChanged(nameof(ProductHasImage)); }
        }

        public bool ProductHasImage { get => StockItem?.Product?.FrontImageLarge != null; }
        
        private async void GetStockItem()
        {
            try
            {
                StockItem = await _homeControlInventoryService.Get(StockItemId);
            }
            catch (Exception ex)
            {
                ToastMessage.LongAlert(ex.Message);
            }
        }
    }
}
