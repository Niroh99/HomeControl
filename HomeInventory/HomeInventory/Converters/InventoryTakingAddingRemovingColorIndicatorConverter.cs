using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace HomeInventory.Converters
{
    public class InventoryTakingAddingRemovingColorIndicatorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 3 && values[0] is bool isAdding && values[1] is bool isRemoving && values[2] is bool isScanning)
            {
                if (!isScanning) return "Gray";

                if (isAdding) return "Lime";
                if (isRemoving) return "Red";
            }

            return "Gray";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
