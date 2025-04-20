using Microsoft.AspNetCore.Mvc.Routing;

namespace HomeControl.Weather
{
    public interface IWeatherService
    {
        Forecast Today { get; }

        bool IsTodaysForecastValid();

        Task EnsureValidTodaysForecastAsync();
    }

    public class WeatherService : IWeatherService
    {
        static WeatherService()
        {
            _httpClient = new HttpClient();
        }

        private readonly static HttpClient _httpClient;

        const string OpenMeteoDateFormat = "yyyy-MM-dd";
        const string OpenMeteoDailyForecastQuery = "sunrise,sunset";
        //const string OpenMeteoForecastUrl = "https://api.open-meteo.com/v1/forecast?latitude=51.3871&longitude=7.7702&daily=sunrise,sunset&timezone=Europe%2FBerlin&start_date=2025-04-20&end_date=2025-04-20";
        const string OpenMeteoForecastUrl = "https://api.open-meteo.com/v1/forecast?";

        public decimal Latitude { get => 51.38m; }

        public decimal Longitude { get => 7.7799993m; }

        public string Timezone { get => "Europe/Berlin"; }

        private Forecast _today;
        public Forecast Today
        {
            get
            {
                if (!IsTodaysForecastValid()) throw new Exception("Todays Forecast is invalid.");

                return _today;
            }
        }

        public bool IsTodaysForecastValid()
        {
            return _today != null && _today.Date == DateOnly.FromDateTime(DateTime.Today);
        }

        public async Task EnsureValidTodaysForecastAsync()
        {
            if (IsTodaysForecastValid()) return;

            var today = DateOnly.FromDateTime(DateTime.Today);

            _today = (await GetForecastsAsync(today, today)).First();
        }

        private async Task<List<Forecast>> GetForecastsAsync(DateOnly from, DateOnly to)
        {
            var uriBuilder = new UriBuilder(OpenMeteoForecastUrl);
            var query = new Dictionary<string, string>()
            {
                { "start_date", from.ToString(OpenMeteoDateFormat) },
                { "end_date", to.ToString(OpenMeteoDateFormat) },
                { "latitude", Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture) },
                { "longitude", Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture) },
                { "timezone", System.Web.HttpUtility.UrlEncode(Timezone) },
                { "daily", OpenMeteoDailyForecastQuery },
            };

            uriBuilder.Query = string.Join('&', query.Select(keyValuePair => $"{keyValuePair.Key}={keyValuePair.Value}"));

            var uri = uriBuilder.Uri.ToString();

            try
            {
                var response = await _httpClient.GetAsync(uriBuilder.Uri);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();

                    return Forecast.FromOpenMeteoForecast(responseJson);
                }
            }
            catch (Exception) { }

            return [];
        }
    }
}