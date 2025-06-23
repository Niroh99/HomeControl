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
    public class ProductsViewModel : BaseViewModel
    {
        public ProductsViewModel()
        {
            Title = "Products";
        }

        private readonly IHomeControlDatabaseModelService _homeControlDatabaseModelService = DependencyService.Get<IHomeControlDatabaseModelService>();

        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();

        public Command RefreshCommand { get => new Command(async () => await RefreshAsync()); }

        private Product _selectedItem;
        public Product SelectedItem
        {
            get { return _selectedItem; }
            set { if (SetProperty(ref _selectedItem, value)) OnSelectedItemChanged(); }
        }

        private async Task RefreshAsync()
        {
            await GetProductsAsync();
        }

        protected override async Task OnViewAppearingAsync()
        {
            await GetProductsAsync();
        }

        private async Task GetProductsAsync()
        {
            await Busy(async () =>
            {
                try
                {
                    var products = await _homeControlDatabaseModelService.Get<Product>();

                    Products.Clear();

                    foreach (var product in products.OrderBy(x => x.Name))
                    {
                        Products.Add(product);
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

            await Shell.Current.GoToAsync($"{nameof(ProductPage)}?{nameof(ProductViewModel.ProductId)}={SelectedItem.Id}");

            SelectedItem = null;
        }
    }
}
