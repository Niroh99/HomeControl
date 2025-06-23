using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace HomeInventory.Views
{
    public class ContentPageBase : ContentPage
    {
        public static readonly BindableProperty AppearingCommandProperty = BindableProperty.Create(nameof(AppearingCommand), typeof(ICommand), typeof(ContentPageBase));
        public ICommand AppearingCommand
        {
            get { return (ICommand)GetValue(AppearingCommandProperty); }
            set { SetValue(AppearingCommandProperty, value); }
        }

        public static readonly BindableProperty BackButtonCommandProperty = BindableProperty.Create(nameof(BackButtonCommand), typeof(ICommand), typeof(ContentPageBase));
        public ICommand BackButtonCommand
        {
            get { return (ICommand)GetValue(BackButtonCommandProperty); }
            set { SetValue(BackButtonCommandProperty, value); }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (AppearingCommand != null && AppearingCommand.CanExecute(null)) AppearingCommand.Execute(null);
        }

        protected override bool OnBackButtonPressed()
        {
            if (BackButtonCommand != null)
            {
                if (!BackButtonCommand.CanExecute(null)) return true;

                BackButtonCommand.Execute(null);
            }

            return base.OnBackButtonPressed();
        }
    }
}