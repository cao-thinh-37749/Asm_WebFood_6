using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Web_food_Asm.Data;
using Models_Asm;
using WebFood.Services;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký dịch vụ
builder.Services.AddSingleton<SendMail>();
builder.Services.AddSingleton<FileImgUpload>();
builder.Services.AddScoped<SessionService>();

// Cấu hình Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Cấu hình DbContext và Identity
builder.Services.AddDbContext<ConnectStr>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectStr")));

builder.Services.AddIdentity<KhachHang, IdentityRole>()
    .AddEntityFrameworkStores<ConnectStr>()
    .AddDefaultTokenProviders();


builder.Services.AddAuthorization(); builder.Services.AddHttpContextAccessor();


// Cấu hình Swagger
builder.Services.AddSwaggerGen(c =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// Thêm CORS vào dịch vụ
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()  // Cho phép tất cả các nguồn
               .AllowAnyMethod()  // Cho phép tất cả các phương thức HTTP (GET, POST, PUT, DELETE...)
               .AllowAnyHeader();  // Cho phép tất cả các header
    });
});


// Đăng ký Controllers
builder.Services.AddControllers();

var app = builder.Build();

// Cấu hình để sử dụng CORS
app.UseCors("AllowAll");  // Sử dụng chính sách "AllowAll"

// Khởi tạo tài khoản mặc định
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.SeedRolesAndUsers(
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(),
        scope.ServiceProvider.GetRequiredService<UserManager<KhachHang>>()
    );
}

// Cấu hình Middleware
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
