using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Essentials;

namespace HomeInventory.Services
{
    public interface ISettingsService
    {
        string Hostname { get; set; }

        int Port { get; set; }

        bool IsValid { get; }
    }

    public class SettingsService : ISettingsService, INotifyPropertyChanged
    {
        private const string HostnameKey = "Hostname";
        public string Hostname
        {
            get => Preferences.Get(HostnameKey, null);
            set
            {
                Preferences.Set(HostnameKey, value);
                OnPropertyChanged(nameof(Hostname));
            }
        }

        private const string PortKey = "Port";
        public int Port
        {
            get => Preferences.Get(PortKey, 5000);
            set
            {
                Preferences.Set(PortKey, value);
                OnPropertyChanged(nameof(Port));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsValid
        {
            get => !string.IsNullOrWhiteSpace(Hostname);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsValid)));
        }
    }
}