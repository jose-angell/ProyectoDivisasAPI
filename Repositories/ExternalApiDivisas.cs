using proyectoDivisas.Models;
using System.Text.Json;

namespace proyectoDivisas.Repositories
{
    public class ExternalApiDivisas
    {
        private readonly HttpClient _httpClient;
        public ExternalApiDivisas(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<Dictionary<string,float>> GetExternalData(string from, string to)
        {
            var result = new Dictionary<string, float>();
            try
            {
                var response = await _httpClient.GetAsync($"/latest?from={from}&to={to}");
                response.EnsureSuccessStatusCode();
                var divisa = await response.Content.ReadAsStringAsync();
                var exchangeRates = JsonDocument.Parse(divisa);
                if (exchangeRates.RootElement.TryGetProperty("rates", out JsonElement ratesElement) &&
                           ratesElement.TryGetProperty(to, out JsonElement rateValue))
                {
                    result[to] = (float)rateValue.GetDouble();
                }
                else
                {
                    result[to] = 0f;
                }
            }catch (Exception ex)
            {
                result[to] = 0f;
            }
            
            return result;
        }
    }
}
