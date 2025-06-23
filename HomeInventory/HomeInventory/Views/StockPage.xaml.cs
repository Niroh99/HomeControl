using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HomeInventory.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StockPage : ContentPageBase
    {
        public StockPage()
        {
            InitializeComponent();
        }
    }
}
