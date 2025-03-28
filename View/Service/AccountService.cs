using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace View.Service
{
    public class AccountService
    {
        private readonly HttpClient _httpClient;

        public AccountService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Account>> GetAccountsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Account>>("accounts/danh-sach");
        }

        public async Task<Account> GetAccountByIdAsync(string id)
        {
            return await _httpClient.GetFromJsonAsync<Account>($"accounts/{id}");
        }

        public async Task<bool> DeleteAccountAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"accounts/{id}");
            return response.IsSuccessStatusCode;
        }
    }

    public class Account
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string HoTen { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string TinhTrang { get; set; }
        public string DiaChi { get; set; }
        public List<string> Roles { get; set; }
    }
}
