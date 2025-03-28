using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using Web_food_Asm.Models;

namespace Web_food_Asm.Data
{
    public class AuthHelper
    {
        private readonly ConnectStr _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SignInManager<KhachHang> _signInManager;
        private readonly UserManager<KhachHang> _userManager;

        public AuthHelper(ConnectStr context, IHttpContextAccessor httpContextAccessor,
                          UserManager<KhachHang> userManager, SignInManager<KhachHang> signInManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // Lấy ID người dùng đã đăng nhập
        public async Task<string?> GetAuthenticatedUserIdAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user != null)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                    return userId;
            }

            var userEmail = _httpContextAccessor.HttpContext?.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
                return null;

            var customer = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Email == userEmail);
            return customer?.Id;
        }

        // Xử lý đăng nhập
        public async Task<(bool Success, string Message, string? UserId, string? Role)> SignInUserAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, "Email không đúng", null, null);

            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
            if (!result.Succeeded)
                return (false, "Mật khẩu không đúng", null, null);

            _httpContextAccessor.HttpContext?.Session.SetString("UserEmail", email);

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.Contains("Admin") ? "Admin" : "Customer";

            return (true, $"Chào {role}!", user.Id, role);
        }

        // Xử lý đăng xuất
        public async Task SignOutUserAsync()
        {
            await _signInManager.SignOutAsync();
            _httpContextAccessor.HttpContext?.Session.Clear();
        }
    }
}
