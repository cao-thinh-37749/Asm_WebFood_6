using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Web_food_Asm.Data;
using Web_food_Asm.Models;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký các dịch vụ
builder.Services.AddSingleton<SendMail>();
builder.Services.AddSingleton<FileImgUpload>();

// Đăng ký DbContext với DI container, cấu hình sử dụng SQL Server với chuỗi kết nối từ appsettings.json
builder.Services.AddDbContext<ConnectStr>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectStr")));

// Đăng ký Identity
builder.Services.AddIdentity<KhachHang, IdentityRole>()
    .AddEntityFrameworkStores<ConnectStr>()
    .AddDefaultTokenProviders();

// Sử dụng cấu hình EmailSettings từ appsettings.json
//builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Tạo tài liệu RESTful API
builder.Services.AddSwaggerGen(c =>
{
    // Tự động lấy XML file mà không cần chỉ rõ đường dẫn
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// Đăng ký dịch vụ Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Thêm dịch vụ điều khiển (Controllers)
builder.Services.AddControllers();

// Cấu hình Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Gọi hàm tạo tài khoản mặc định
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.SeedRolesAndUsers(
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(),
        scope.ServiceProvider.GetRequiredService<UserManager<KhachHang>>()
    );
}

// Cấu hình HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSession();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
