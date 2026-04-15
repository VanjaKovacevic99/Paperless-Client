using HCI_2025_Project_Template.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
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

        public async Task<ApiResponse<Document>> GetDocumentsAsync(int page, int pageSize)
        {
            var response = await _httpClient.GetAsync(
                $"documents/?page={page}&page_size={pageSize}");

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse<Document>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<List<Tag>> GetTagsAsync()
        {
            var response = await _httpClient.GetAsync("tags/");
            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var apiResponse = JsonSerializer.Deserialize<ApiResponse<Tag>>(json, options);

            return apiResponse?.results ?? new List<Tag>();


        }

        public async Task<List<DocumentType>> GetDocumentTypesAsync()
        {
            var response = await _httpClient.GetAsync("document_types/");
            var json = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonSerializer.Deserialize<ApiResponse<DocumentType>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return apiResponse?.results ?? new List<DocumentType>();
        }
    }
}