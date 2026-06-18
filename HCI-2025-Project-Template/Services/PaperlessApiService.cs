using HCI_2025_Project_Template.Models;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
        public async Task<List<Document>> GetAllDocumentsAsync()
        {
            var allDocuments = new List<Document>();

            int page = 1;

            while (true)
            {
                var response = await _httpClient.GetAsync(
                    $"documents/?page={page}&page_size=100");

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<ApiResponse<Document>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (result?.results == null || result.results.Count == 0)
                    break;

                allDocuments.AddRange(result.results);

                if (string.IsNullOrEmpty(result.next))
                    break;

                page++;
            }


            return allDocuments;
        }

        public async Task<Dictionary<string, int>> GetDocumentTypeStatsAsync()
        {
            var stats = new Dictionary<string, int>();

            int page = 1;
            const int pageSize = 100;

            while (true)
            {
                var response = await _httpClient.GetAsync(
                    $"documents/?page={page}&page_size={pageSize}");

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<ApiResponse<Document>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (result?.results == null || result.results.Count == 0)
                    break;

                foreach (var doc in result.results)
                {
                    var typeName = doc.DocumentTypeName ?? "Unknown";

                    if (stats.ContainsKey(typeName))
                        stats[typeName]++;
                    else
                        stats[typeName] = 1;
                }

                if (string.IsNullOrEmpty(result.next))
                    break;

                page++;
            }

            return stats;
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
        public async Task<List<Correspondent>> GetCorrespondentsAsync()
        {


            var response = await _httpClient.GetAsync("correspondents/");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();


            var apiResponse = JsonSerializer.Deserialize<ApiResponse<Correspondent>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return apiResponse?.results ?? new List<Correspondent>();
        }

        public async Task<ApiResponse<Document>> GetDocumentsAsync(int page, int pageSize, string search, int? tagId, int? typeId, int? correspondentId)
        {
            var url = $"documents/?page={page}&page_size={pageSize}";

            if (!string.IsNullOrWhiteSpace(search))
                url += $"&query={search}";

            if (tagId.HasValue)
                url += $"&tags__id={tagId}";

            if (typeId.HasValue)
                url += $"&document_type__id={typeId}";

            if (correspondentId.HasValue)
                url += $"&correspondent__id={correspondentId}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ApiResponse<Document>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }


        public async Task<byte[]> DownloadDocumentAsync(int id)
        {
            var url = $"http://paperless-etfbl.duckdns.org:8000/api/documents/{id}/download/";

            return await _httpClient.GetByteArrayAsync(url);
        }


        public async Task<bool> UploadDocumentAsync(string filePath, string title, int? tagId, int? typeId, int? correspondentId)
        {


            using var form = new MultipartFormDataContent();


            var fileBytes = await File.ReadAllBytesAsync(filePath);
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            form.Add(fileContent, "document", Path.GetFileName(filePath));


            form.Add(new StringContent(title ?? ""), "title");

            if (tagId.HasValue)
                form.Add(new StringContent(tagId.ToString()), "tags");

            if (typeId.HasValue)
                form.Add(new StringContent(typeId.ToString()), "document_type");

            if (correspondentId.HasValue)
                form.Add(new StringContent(correspondentId.ToString()), "correspondent");

            var response = await _httpClient.PostAsync("documents/post_document/", form);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CreateTagAsync(string name, string color)
        {
            try
            {
                var payload = new { name, color };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("tags/", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteTagAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"tags/{id}/");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateTagAsync(int id, string newName, string color)
        {
            try
            {
                var payload = new
                {
                    name = newName,
                    color = color
                };
                MessageBox.Show(color);
                var json = JsonSerializer.Serialize(payload);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"tags/{id}/", content);

                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"ERROR: {response.StatusCode}\n{responseBody}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
    }
}