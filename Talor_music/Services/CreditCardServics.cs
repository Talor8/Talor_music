using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
namespace Talor_music.Services
{
    public class CreditCardServics
    {
        private readonly HttpClient _httpClient;

        public CreditCardServics(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CreditCardServics?> GetCreditCardServicsAsync(string date, string city)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<CreditCardServics>($"api/weather/date/{date}?city={Uri.EscapeDataString(city)}");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }
    }
}
