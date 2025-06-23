using HomeInventory.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Forms;

namespace HomeInventory.Integrations.HomeControl
{
    public interface IHomeControlDatabaseModelService
    {
        Task<T> Post<T>(T model) where T : class;

        Task<T> GetById<T>(object id) where T : class;

        Task<List<T>> Get<T>(Dictionary<string, string> query = null);

        Task Delete<T>(object id) where T : class;
    }

    public class HomeControlDatabaseModelService : HomeControlService, IHomeControlDatabaseModelService
    {
        public async Task<T> Post<T>(T model) where T : class
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var response = await HttpClient.PostAsync(BuildAddress(typeof(T).Name), new StringContent(System.Text.Json.JsonSerializer.Serialize(model), Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode) throw new Exception(response.StatusCode.ToString());

            return await DeserializeResponseContent<T>(response);
        }

        public async Task<T> GetById<T>(object id) where T : class
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            var response = await HttpClient.GetAsync(BuildAddress($"{typeof(T).Name}/{id}"));

            if (!response.IsSuccessStatusCode) throw new Exception(response.StatusCode.ToString());

            return await DeserializeResponseContent<T>(response);
        }

        public async Task<List<T>> Get<T>(Dictionary<string, string> query = null)
        {
            var queryString = "";

            if (query != null && query.Count > 0) queryString = $"?{string.Join("&", query.Select(queryParameter => $"{HttpUtility.UrlEncode(queryParameter.Key)}={HttpUtility.UrlEncode(queryParameter.Value)}"))}";

            var address = BuildAddress($"{typeof(T).Name}{queryString}");

            var response = await HttpClient.GetAsync(BuildAddress($"{typeof(T).Name}{queryString}"));

            if (!response.IsSuccessStatusCode) throw new Exception(response.StatusCode.ToString());

            return await DeserializeResponseContent<List<T>>(response);
        }

        public async Task Delete<T>(object id) where T : class
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            var response = await HttpClient.DeleteAsync(BuildAddress($"{typeof(T).Name}/{id}"));

            if (!response.IsSuccessStatusCode) throw new Exception(response.StatusCode.ToString());
        }

        protected override string RequestPrefix() => "/Data";
    }
}