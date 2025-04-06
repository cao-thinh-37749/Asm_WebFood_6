using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Models_Asm;
using System.Text.Json;
using System;

namespace Web_Food_Asm_UI.Services
{
    public class HomeService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "https://localhost:44373/api/Home_API";
        public HomeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<List<DanhMucSanPham>?> GetCategoriesAsync()
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/categories-dropdown");

            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<List<DanhMucSanPham>>();
        }

        // Gọi API để lấy danh sách sản phẩm với filter
        public async Task<List<SanPham>?> GetProductsAsync(int? categoryId = null, decimal? minPrice = null, decimal? maxPrice = null, int page = 1, int pageSize = 8)
        {
            var url = $"{_apiBaseUrl}/index?page={page}&pageSize={pageSize}";

            if (categoryId.HasValue) url += $"&categoryId={categoryId}";
            if (minPrice.HasValue) url += $"&minPrice={minPrice}";
            if (maxPrice.HasValue) url += $"&maxPrice={maxPrice}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Lỗi: {response.StatusCode} - {response.ReasonPhrase}");
                    return null;
                }

                // Đọc phản hồi JSON thành danh sách sản phẩm từ response.Content
                return await response.Content.ReadFromJsonAsync<List<SanPham>>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gọi API: {ex.Message}");
                return null;
            }
        }




        // Gọi API để lấy chi tiết sản phẩm
        public async Task<SanPham?> GetProductDetailsAsync(int maSanPham)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<SanPham>($"{_apiBaseUrl}/details/{maSanPham}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gọi GetProductDetailsAsync: {ex.Message}");
                return null;
            }
        }
    }
}
