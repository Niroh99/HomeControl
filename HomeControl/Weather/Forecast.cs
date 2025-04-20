using System.Text.Json.Nodes;

namespace HomeControl.Weather
{
    public class Forecast
    {
        public static List<Forecast> FromOpenMeteoForecast(string json)
        {
            var jsonDocument = System.Text.Json.JsonDocument.Parse(json);

            var forecasts = new List<Forecast>();

            var dailyProperty = jsonDocument.RootElement.GetProperty("daily");

            var timeProperty = dailyProperty.GetProperty("time");
            var sunriseProperty = dailyProperty.GetProperty("sunrise");
            var sunsetProperty = dailyProperty.GetProperty("sunset");

            var entryCount = timeProperty.GetArrayLength();

            for (int i = 0; i < entryCount; i++)
            {
                var forecast = new Forecast()
                {
                    Date = DateOnly.Parse(timeProperty[i].GetString()),
                    Sunrise = TimeOnly.FromDateTime(DateTime.Parse(sunriseProperty[i].GetString())),
                    Sunset = TimeOnly.FromDateTime(DateTime.Parse(sunsetProperty[i].GetString())),
                };

                forecasts.Add(forecast);
            }

            return forecasts;
        }

        public DateOnly Date { get; set; }

        public TimeOnly Sunrise { get; set; }

        public TimeOnly Sunset { get; set; }
    }
}
