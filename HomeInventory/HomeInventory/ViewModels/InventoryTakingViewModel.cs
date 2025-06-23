using BarcodeScanner.Mobile;
using HomeInventory.Integrations.HomeControl;
using HomeInventory.Integrations.OpenFoodFacts;
using HomeInventory.Models;
using HomeInventory.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace HomeInventory.ViewModels
{
    public class InventoryTakingViewModel : BaseViewModel
    {
        public class ScanHistoryItem
        {
            public ScanHistoryItem(string productName, decimal newQuantity, decimal movemedAmount)
            {
                ProductName = productName;
                NewQuantity = newQuantity;
                MovedAmount = movemedAmount;
            }

            public string ProductName { get; }

            public decimal NewQuantity { get; }

            public decimal MovedAmount { get; }

            public string MovedAmountDisplay { get => MovedAmount > 0 ? $"+{MovedAmount}" : MovedAmount.ToString(); }

            public string MovedAmountColor { get => MovedAmount > 0 ? "Lime" : "Red"; }
        }

        public InventoryTakingViewModel()
        {
            Title = "Take Inventory";

            RestartScanningCommand = new Command(async (parameter) => await OnRestartScanning());
            BarcodeDetected = new Command(async (parameter) => await OnBarcodeDetected(parameter));
        }

        private readonly IOpenFoodFactsService _openFoodFactsService = DependencyService.Get<IOpenFoodFactsService>();
        private readonly IHomeControlDatabaseModelService _homeControlDatabaseModelService = DependencyService.Get<IHomeControlDatabaseModelService>();
        private readonly IHomeControlInventoryService _homeControlInventoryService = DependencyService.Get<IHomeControlInventoryService>();

        private string _lastScannedBarcode;
        private DateTime _lastScannedTime;

        public Command RestartScanningCommand { get; }
        public Command BarcodeDetected { get; }

        private bool _isAdding = true;
        public bool IsAdding
        {
            get { return _isAdding; }
            set { SetProperty(ref _isAdding, value); }
        }

        private bool _isRemoving;
        public bool IsRemoving
        {
            get { return _isRemoving; }
            set { SetProperty(ref _isRemoving, value); }
        }

        private bool _isScanning;
        public bool IsScanning
        {
            get { return _isScanning; }
            set { SetProperty(ref _isScanning, value); }
        }

        private Product _product;
        public Product Product
        {
            get { return _product; }
            set { if (SetProperty(ref _product, value)) OnPropertyChanged(nameof(HasProduct)); }
        }

        public bool HasProduct { get => Product != null; }

        private Location _location;
        public Location Location
        {
            get { return _location; }
            set { SetProperty(ref _location, value); }
        }

        public ObservableCollection<ScanHistoryItem> ScanHistory {  get; } = new ObservableCollection<ScanHistoryItem>();

        protected override async Task OnViewAppearingAsync()
        {
            await Methods.AskForRequiredPermission();
            
            try
            {
                var locations = await _homeControlDatabaseModelService.Get<Location>();
                
                Location = locations.FirstOrDefault();
            }
            catch (Exception ex)
            {
                ToastMessage.LongAlert(ex.Message);
            }

            await StartScanning().ConfigureAwait(false);
        }

        private async Task StartScanning()
        {
            await Task.Delay(3000);

            IsScanning = true;
        }

        private async Task ReStartScanning()
        {
            await Task.Delay(500);

            IsScanning = true;
        }

        private async Task OnRestartScanning()
        {
            IsScanning = false;

            await ReStartScanning().ConfigureAwait(false);
        }

        private async Task OnBarcodeDetected(object parameter)
        {
            var detectedEventArgs = (OnDetectedEventArg)parameter;

            foreach (var code in detectedEventArgs.BarcodeResults.Select(x =>x.DisplayValue)) await HandleBarcode(code);

            await ReStartScanning().ConfigureAwait(false);
        }

        private async Task HandleBarcode(string code)
        {
            if (code == _lastScannedBarcode)
            {
                var now = DateTime.Now;
                if (now - _lastScannedTime < TimeSpan.FromSeconds(3)) return;
            }

            Xamarin.Essentials.Vibration.Vibrate();

            _lastScannedBarcode = code;
            _lastScannedTime = DateTime.Now;

            await Busy(async () =>
            {
                if (IsAdding)
                {
                    try
                    {
                        var product = await GetOrAddProduct(code);

                        await BookStock(product, 1);

                        Product = product;
                    }
                    catch (Exception ex)
                    {
                        ToastMessage.LongAlert(ex.Message);
                    }
                }
                else if (IsRemoving)
                {
                    try
                    {
                        var product = await GetHomeControlProduct(code);

                        if (product == null) return;

                        await BookStock(product, -1);

                        Product = product;
                    }
                    catch (Exception ex)
                    {
                        ToastMessage.LongAlert(ex.Message);
                    }
                }
            });
        }

        private async Task BookStock(Product product, int movedAmount)
        {
            var stock = await _homeControlInventoryService.BookStock(product.Id, Location.Id, movedAmount);

            InsertScanHistoryItem(stock, movedAmount);
        }

        private void InsertScanHistoryItem(Stock stock, decimal movedAmount)
        {
            ScanHistory.Insert(0, new ScanHistoryItem(stock.Product.Name, stock.Quantity, movedAmount));
        }

        private async Task<Product> GetOrAddProduct(string code)
        {
            Product homeControlProduct = await GetHomeControlProduct(code);

            if (homeControlProduct != null) return homeControlProduct;

            var openFoodFactsProductResponse = new GetProductResponse();

            if (await _openFoodFactsService.TryGetProduct(code, openFoodFactsProductResponse))
            {
                var product = openFoodFactsProductResponse.Product;

                return await _homeControlDatabaseModelService.Post(product);
            }

            throw new Exception("Unknown Product.");
        }

        private async Task<Product> GetHomeControlProduct(string code)
        {
            return (await _homeControlDatabaseModelService.Get<Product>(new Dictionary<string, string>() { { nameof(Product.GlobalTradeItemNumber), code } })).FirstOrDefault();
        }
    }
}