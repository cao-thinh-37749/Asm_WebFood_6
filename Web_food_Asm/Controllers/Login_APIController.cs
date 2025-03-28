using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Web_food_Asm.Models;
using Web_food_Asm.Models.ViewModel;

namespace Web_food_Asm.Controllers
{
    [ApiController]
    [Route("api/login")]
    public class Login_APIController : ControllerBase
    {
        private readonly SignInManager<KhachHang> _signInManager;
        private readonly UserManager<KhachHang> _userManager;

        public Login_APIController(UserManager<KhachHang> userManager, SignInManager<KhachHang> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Xử lý đăng nhập người dùng.
        /// </summary>
        /// <param name="model">Thông tin đăng nhập (Email, Mật khẩu).</param>
        /// <returns>Trả về thông báo chào mừng tùy theo quyền (Admin hoặc Khách hàng).</returns>
        /// <response code="200">Đăng nhập thành công</response>
        /// <response code="400">Dữ liệu nhập vào không hợp lệ</response>
        /// <response code="401">Sai tài khoản hoặc mật khẩu</response>
        [HttpPost]
        public async Task<IActionResult> Login([FromForm] Login_ViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized("Email không đúng");

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (!result.Succeeded)
                return Unauthorized("Mật khẩu không đúng");

            // Lưu email vào session
            HttpContext.Session.SetString("UserEmail", model.Email);

            // Lấy danh sách quyền của người dùng
            var roles = await _userManager.GetRolesAsync(user);

            // Kiểm tra nếu là admin hay khách hàng
            if (roles.Contains("Admin"))
                return Ok(new { message = "Chào Admin!", userId = user.Id, role = "Admin" });
            else
                return Ok(new { message = "Chào Khách hàng!", userId = user.Id, role = "Customer" });

        }
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.Clear(); // 🚀 Xóa session
            return Ok(new { message = "Đã đăng xuất thành công." });
        }

    }
}
