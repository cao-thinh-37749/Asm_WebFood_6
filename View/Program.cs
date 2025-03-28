using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace View
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            // Chỉ đăng ký HttpClient một lần
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:44373/api/accounts/danh-sach") });

            await builder.Build().RunAsync();
        }
    }
}
