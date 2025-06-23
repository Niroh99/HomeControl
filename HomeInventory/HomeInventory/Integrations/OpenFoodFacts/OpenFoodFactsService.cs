using HomeInventory.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HomeInventory.Integrations.OpenFoodFacts
{
    public interface IOpenFoodFactsService
    {
        Task<bool> TryGetProduct(string code, GetProductResponse response);
    }

    public class OpenFoodFactsService : IOpenFoodFactsService
    {
        public string Language { get; set; } = "de";

        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task<bool> TryGetProduct(string code, GetProductResponse response)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentNullException(nameof(code));
            if (response == null) throw new ArgumentNullException(nameof(response));

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"https://world.openfoodfacts.net/api/v2/product/{code}.json");

            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "off:off");

            var httpResponse = await _httpClient.SendAsync(httpRequest);

            if (!httpResponse.IsSuccessStatusCode) return false;

            var responseModel = await JsonSerializer.DeserializeAsync<ResponseModel>(await httpResponse.Content.ReadAsStreamAsync(), JsonSerializerOptions.Web);

            response.Status = responseModel.Status;
            response.StatusVerbose = responseModel.SatusVerbose;

            if (responseModel.Status != 1) return false;

            if (responseModel.Product?.SelectedImages?.Front?.Display.TryGetValue(Language, out var frontImageLarge) != true) frontImageLarge = null;
            if (responseModel.Product?.SelectedImages?.Front?.Thumb.TryGetValue(Language, out var frontImageSmall) != true) frontImageSmall = null;
            if (responseModel.Product?.SelectedImages?.Nutrition?.Display.TryGetValue(Language, out var nutritionImage) != true) nutritionImage = null;

            response.Product = new Product
            {
                Name = responseModel.Product.Name,
                GlobalTradeItemNumber = responseModel.Product?.Code,
                Brands = responseModel.Product?.Brands,
                FrontImageLarge = frontImageLarge,
                FrontImageSmall = frontImageSmall,
                NutritionImage = nutritionImage,
            };

            return true;
        }
    }
}