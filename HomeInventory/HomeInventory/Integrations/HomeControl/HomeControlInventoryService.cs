using HomeInventory.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace HomeInventory.Integrations.HomeControl
{
    public interface IHomeControlInventoryService
    {
        Task<List<Stock>> Get();

        Task<Stock> Get(int id);

        Task<Stock> BookStock(int productId, int locationId, decimal quantity);
    }

    public class HomeControlInventoryService : HomeControlService, IHomeControlInventoryService
    {
        public async Task<List<Stock>> Get()
        {
            var response = await HttpClient.GetAsync(BuildAddress());

            if (!response.IsSuccessStatusCode) throw new Exception(response.StatusCode.ToString());

            return await DeserializeResponseContent<List<Stock>>(response);
        }

        public async Task<Stock> Get(int id)
        {
            var response = await HttpClient.GetAsync(BuildAddress($"{id}"));

            if (!response.IsSuccessStatusCode) throw new Exception(response.StatusCode.ToString());

            return await DeserializeResponseContent<Stock>(response);
        }

        public async Task<Stock> BookStock(int productId, int locationId, decimal quantity)
        {
            var response = await HttpClient.PostAsync(BuildAddress("BookStock"), new StringContent(System.Text.Json.JsonSerializer.Serialize(new
            {
                ProductId = productId,
                LocationId = locationId,
                Quantity = quantity
            }), Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode) throw new Exception(response.StatusCode.ToString());

            return await DeserializeResponseContent<Stock>(response);
        }

        protected override string RequestPrefix() => "/Inventory";
    }
}