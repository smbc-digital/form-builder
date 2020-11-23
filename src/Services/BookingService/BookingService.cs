using form_builder.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace form_builder.Services.BookingService
{
    public class BookingService : IBookingService
    {
        #region Fields & Constructor
        private readonly HttpClient _httpClient;
        private readonly BookingApiConfiguration _apiConfig;
        public BookingService(HttpClient httpClient, IOptions<BookingApiConfiguration> apiConfig) 
        {
            _httpClient = httpClient;
            _apiConfig = apiConfig.Value;
        }
        #endregion

        #region Public Methods
        public async Task<List<AvailabilityDayResponse>> GetAvailability() => await GetAsync<List<AvailabilityDayResponse>>("/Availability");
        #endregion

        #region Private Methods

        // TODO: PATCH

        // TODO : DELETE

        private async Task<T> PostAsync<T>(string apiEndpoint, string body)
        {
            T response = default(T);

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                //_httpClient.DefaultRequestHeaders.Add();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.ConnectionClose = false;

                Uri uri = new Uri(apiBase + apiEndpoint);
                StringContent content = new StringContent(body, Encoding.UTF8, "application/json");
                HttpResponseMessage apiResponse = await _httpClient.PostAsync(uri, content);
                apiResponse.EnsureSuccessStatusCode();

                string responseMessage = await apiResponse.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseMessage))
                {
                    response = JsonSerializer.Deserialize<T>(responseMessage, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                content.Dispose();
            }
            catch
            {

            }
            finally
            {

            }

            return response;
        }

        private async Task<T> GetAsync<T>(string apiEndpoint)
        {
            T response = default(T);
            if (_httpClient == null) return response;

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                //_httpClient.DefaultRequestHeaders.Add(); // Token, from...?
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.ConnectionClose = false;

                Uri uri = new Uri(_apiConfig.BaseAddress + apiEndpoint);
                HttpResponseMessage apiResponse = await _httpClient.GetAsync(uri);
                apiResponse.EnsureSuccessStatusCode();

                string responseMessage = await apiResponse.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseMessage))
                {
                    response = JsonSerializer.Deserialize<T>(responseMessage, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            catch
            {

            }
            finally
            {

            }

            return response;
        }
        #endregion
    }
}
