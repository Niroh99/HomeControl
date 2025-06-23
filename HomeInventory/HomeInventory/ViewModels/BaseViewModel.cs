using HomeInventory.Models;
using HomeInventory.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HomeInventory.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public BaseViewModel()
        {
            ViewAppearing = new Command(async () => await OnViewAppearingAsync());
        }

        protected IToastMessage ToastMessage { get; } = DependencyService.Get<IToastMessage>();

        public Command ViewAppearing { get; }

        bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            private set { SetProperty(ref _isBusy, value); }
        }

        string _title = string.Empty;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        protected virtual async Task OnViewAppearingAsync()
        {
            await Task.CompletedTask;
        }

        protected void Busy(Action whiteBusy)
        {
            IsBusy = true;

            try
            {
                whiteBusy();
            }
            catch
            {
                throw;
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected async Task Busy(Func<Task> whiteBusy)
        {
            IsBusy = true;

            try
            {
                await whiteBusy();
            }
            catch
            {
                throw;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
