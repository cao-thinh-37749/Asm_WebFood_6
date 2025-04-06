using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading.Tasks;
using Web_food_Asm.Data;
using Models_Asm;

namespace Web_food_Asm.Controllers
{
    /// <summary>
    /// API xử lý đăng ký tài khoản
    /// </summary>
    [ApiController]
    [Route("api/register")]
    public class Register_APIController : ControllerBase
    {
        private readonly UserManager<KhachHang> _userManager;
        private readonly SendMail _sendMail;

        public Register_APIController(UserManager<KhachHang> userManager, SendMail sendMail)
        {
            _userManager = userManager;
            _sendMail = sendMail;
        }

        #region Đăng ký
        /// <summary>
        /// Đăng ký tài khoản mới
        /// </summary>
        /// <param name="model">Thông tin đăng ký gồm email, mật khẩu</param>
        /// <returns>Kết quả đăng ký</returns>
        /// <response code="200">Đăng ký thành công</response>
        /// <response code="400">Dữ liệu không hợp lệ</response>
        /// <response code="409">Email đã tồn tại</response>
        [HttpPost]
        public async Task<IActionResult> Register([FromForm] Register_ViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Error = "Dữ liệu không hợp lệ" });

            // Kiểm tra email đã tồn tại
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest(new { Error = "Email đã tồn tại" });

            // Kiểm tra số điện thoại đã tồn tại
            var existingPhone = await _userManager.Users.AnyAsync(u => u.PhoneNumber == model.PhoneNumber);
            if (existingPhone)
                return BadRequest(new { Error = "Số điện thoại đã tồn tại" });

            var user = new KhachHang
            {
                HoTen = model.HoTen,
                UserName = model.Email,
                Email = model.Email,
                PasswordHash = model.Password,
                PhoneNumber = model.PhoneNumber,
                DiaChi = model.DiaChi,
            };


            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(new { Error = "Đăng ký thất bại", Details = result.Errors });

            // Gửi mật khẩu về email
            string subject = "Chào mừng bạn đến với WebFood Quốc Thịnh";
            string body = $"Chào mừng {model.Email} đã đến với WebFood Quốc Thịnh, mật khẩu của bạn là: {model.Password}";
            try
            {
                _sendMail.SendEmail(model.Email, subject, body);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Lỗi khi gửi email", Details = ex.Message });
            }

            // Cấp quyền (role) cho người dùng
            var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
            return Ok(new { Message = "Đăng ký thành công!" });
        }
        #endregion

        #region Quên mật khẩu và đặc lại mật khẩu
        /// <summary>
        /// Gửi email quên mật khẩu
        /// </summary>
        /// <param name="model">Email của tài khoản cần đặt lại mật khẩu</param>
        /// <returns>Kết quả xử lý</returns>
        /// <response code="200">Email đặt lại mật khẩu đã được gửi</response>
        /// <response code="400">Dữ liệu không hợp lệ</response>
        /// <response code="404">Email không tồn tại</response>
        [HttpPost("quen-mat-khau")]
        public async Task<IActionResult> ForgotPassword([FromForm] ForgotPassword_ViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Error = "Dữ liệu không hợp lệ" });

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return NotFound(new { Error = "Email không tồn tại trong hệ thống" });

            // Tạo token reset mật khẩu
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // khi nào có font end quên mât khẩu thay đường dẫn vào
            string resetLink = $"https://localhost:44373/api/dangky/XacThucQuenMatKhau?email={WebUtility.UrlEncode(model.Email)}&token={WebUtility.UrlEncode(token)}";

            string subject = "Yêu cầu đặt lại mật khẩu";
            string body = $"Mã xác thực của bạn để đặt lại mật khẩu: {token}. Vui lòng sử dụng API /api/dangky/DatLaiMatKhau để đặt lại mật khẩu.";
            try
            {
                _sendMail.SendEmail(model.Email, subject, body);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Lỗi khi gửi email", Details = ex.Message });
            }
            return Ok(new { Message = "Email đặt lại mật khẩu đã được gửi!" });
        }

        /// <summary>
        /// API để đặt lại mật khẩu mới
        /// </summary>
        /// <param name="model">Thông tin đặt lại mật khẩu</param>
        /// <returns>Kết quả xử lý</returns>
        /// <response code="200">Đặt lại mật khẩu thành công</response>
        /// <response code="400">Dữ liệu không hợp lệ</response>
        /// <response code="404">Tài khoản không tồn tại</response>
        [HttpPost("dat-lai-mat-khau")]
        public async Task<IActionResult> ResetPassword([FromForm] ResetPassword_ViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Error = "Dữ liệu không hợp lệ" });

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return NotFound(new { Error = "Email không tồn tại trong hệ thống" });

            // Đặt lại mật khẩu bằng token
            // token lấy từ email khi nào dùng quên mật khẩu mới có thể lấy được token từ mail
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!result.Succeeded)
                return BadRequest(new { Error = "Đặt lại mật khẩu thất bại", Details = result.Errors });

            string subject = "Yêu cầu đặt lại mật khẩu";
            string body = $"Bạn đã đổi mật khẩu thành công. Mật khẩu mới của {user.Email} là: {model.NewPassword}";
            try
            {
                _sendMail.SendEmail(model.Email, subject, body);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Lỗi khi gửi email", Details = ex.Message });
            }
            return Ok(new { Message = "Đặt lại mật khẩu thành công!" });
        }
        #endregion
    }
}
