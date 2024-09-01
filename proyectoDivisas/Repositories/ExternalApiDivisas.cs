namespace proyectoDivisas.Repositories
{
    public class ExternalApiDivisas
    {
        private readonly HttpClient _httpClient;
        public ExternalApiDivisas(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<string> GetExternalData(string endpint)
        {
            var response = await _httpClient.GetAsync(endpint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
