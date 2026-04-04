using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;

namespace HCI_2025_Project.Services
{
    public class PaperlessApiService
    {
        private readonly HttpClient _httpClient;

        public PaperlessApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://paperless-etfbl.duckdns.org:8000/api/");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Token", "88bf3a2b5e1d113067e974612e0a6bf73bb35e67");
        }

        public async Task<string> GetDocumentsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("documents/");
                var content = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"duzina" + content.Length);
                return $"STATUS: {response.StatusCode}\n\n{content}";
            }
            catch (Exception ex)
            {
                return "EXCEPTION: " + ex.Message;
            }
        }
    }
}