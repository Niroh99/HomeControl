using HomeInventory.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HomeInventory.Integrations.HomeControl
{
    public abstract class HomeControlService
    {
        static HomeControlService()
        {
            _httpClient = new HttpClient();
        }

        private static readonly HttpClient _httpClient;
        protected static HttpClient HttpClient { get => _httpClient; }

        private static readonly ISettingsService _settingsService = DependencyService.Get<ISettingsService>();

        protected virtual string RequestPrefix() => "";

        protected string BuildAddress(string requestUri = "")
        {
            return $"http://{_settingsService.Hostname}:{_settingsService.Port}{RequestPrefix()}/{requestUri}";
        }

        protected async Task<T> DeserializeResponseContent<T>(HttpResponseMessage response)
        {
            return await System.Text.Json.JsonSerializer.DeserializeAsync<T>(await response.Content.ReadAsStreamAsync(), System.Text.Json.JsonSerializerOptions.Web);
        }
    }
}