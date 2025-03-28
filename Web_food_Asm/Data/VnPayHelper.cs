using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Web_food_Asm.Controllers;

namespace Web_food_Asm.Data
{
    public static class VnPayHelper
    {
        public static string CreatePaymentUrl(int orderId, decimal amount)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            string vnp_Url = config["VnPay:Url"];
            string vnp_TmnCode = config["VnPay:TmnCode"];
            string vnp_HashSecret = config["VnPay:HashSecret"];
            string returnUrl = config["VnPay:ReturnUrl"];

            var time = DateTime.UtcNow.AddHours(7);
            var vnp_Params = new SortedList<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", vnp_TmnCode },
                { "vnp_Amount", (amount * 100).ToString() },
                { "vnp_CreateDate", time.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", "VND" },
                { "vnp_IpAddr", "127.0.0.1" },
                { "vnp_Locale", "vn" },
                { "vnp_OrderInfo", $"Thanh toán đơn hàng {orderId}" },
                { "vnp_OrderType", "billpayment" },
                { "vnp_ReturnUrl", returnUrl },
                { "vnp_TxnRef", orderId.ToString() }
            };

            string queryString = string.Join("&", vnp_Params.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
            string secureHash = CreateSecureHash(queryString, vnp_HashSecret);
            return $"{vnp_Url}?{queryString}&vnp_SecureHash={secureHash}";
        }

        internal static bool ValidateSignature(VnPayReturnModel vnpayData)
        {
            throw new NotImplementedException();
        }

        private static string CreateSecureHash(string data, string secretKey)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToUpper();
        }
    }

    public class PaymentService
    {
        private readonly HttpClient _httpClient;

        public PaymentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> AuthenticateAndPay(string paymentMethod, string username, string password, decimal amount)
        {
            var requestData = new
            {
                paymentMethod,
                username,
                password,
                amount
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("https://sandbox.vnpayment.vn/api/auth-pay", requestData);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    return result?.Success ?? false;
                }
                else
                {
                    Console.WriteLine($"Lỗi HTTP: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gọi API VNPAY: {ex.Message}");
            }

            return false;
        }

        // Định nghĩa class để ánh xạ JSON response
        public class ApiResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }
    }
}
