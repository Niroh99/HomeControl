using HomeInventory.Integrations.HomeControl;
using HomeInventory.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HomeInventory.ViewModels
{
    [QueryProperty(nameof(ProductId), nameof(ProductId))]
    public class ProductViewModel : BaseViewModel
    {
        public ProductViewModel()
        {
            Images.CollectionChanged += Images_CollectionChanged;
        }

        private readonly IHomeControlDatabaseModelService _homeControlDatabaseModelService = DependencyService.Get<IHomeControlDatabaseModelService>();

        private int _productId;
        public int ProductId
        {
            get { return _productId; }
            set { if (SetProperty(ref _productId, value)) GetProduct(); }
        }

        private Product _product;
        public Product Product
        {
            get { return _product; }
            private set { SetProperty(ref _product, value); }
        }
        
        public bool HasImages { get => Images.Count > 0; }

        public ObservableCollection<string> Images { get; } = new ObservableCollection<string>();

        private void Images_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasImages));
        }

        private async void GetProduct()
        {
            try
            {
                Product = await _homeControlDatabaseModelService.GetById<Product>(ProductId);

                if (!string.IsNullOrWhiteSpace(Product.FrontImageLarge)) Images.Add(Product.FrontImageLarge);
                if (!string.IsNullOrWhiteSpace(Product.NutritionImage)) Images.Add(Product.NutritionImage);
            }
            catch (Exception ex)
            {
                ToastMessage.LongAlert(ex.Message);
            }
        }
    }
}
