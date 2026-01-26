using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HCI_2025_Project_Template.Services
{
    internal class PaperlessApiService
    {
        private readonly HttpClient _httpClient;

        private const string BaseUrl = "http://paperless-etfbl.duckdns.org:8000/api/";

        public PaperlessApiService(string apiToken)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Token", apiToken);
        }

        /// <summary>
        /// Provjera da li je API token validan
        /// </summary>
        public async Task<bool> TestLoginAsync()
        {
            try
            {
                // Jednostavan endpoint koji zahtijeva autentifikaciju
                var response = await _httpClient.GetAsync("documents/");

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

    }
}
