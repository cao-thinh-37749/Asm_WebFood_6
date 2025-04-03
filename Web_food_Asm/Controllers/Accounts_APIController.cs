using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_food_Asm.Data;
using Web_food_Asm.Models;
using Web_food_Asm.Models.ViewModel;
using WebFood.Services;

namespace Web_food_Asm.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    [AuthorizeUser]
    public class Accounts_APIController : SessionService
    {
        private readonly UserManager<KhachHang> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SendMail _sendMail;
        private readonly FileImgUpload _fileImgUpload;
        private readonly ConnectStr _context;
        public Accounts_APIController(UserManager<KhachHang> userManager, RoleManager<IdentityRole> roleManager, SendMail sendMail, FileImgUpload fileImgUpload, ConnectStr context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _sendMail = sendMail;
            _fileImgUpload = fileImgUpload;
            _context = context;
        }

        #region Danh Sách Tài khoản
        /// <summary>
        /// Lấy danh sách tài khoản kèm theo vai trò
        /// </summary>
        [HttpGet("danh-sach")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            // Dùng Dictionary để lưu vai trò của từng user
            var userRoles = new Dictionary<string, List<string>>();

            foreach (var user in users)
            {
                //giúp tạo một danh sách Dictionary chứa ID của mỗi user và các vai trò tương ứng của họ.
                userRoles[user.Id] = (await _userManager.GetRolesAsync(user)).ToList();
            }

            var userList = users.Select(user => new
            {
                Hinh = string.IsNullOrEmpty(user.Hinh) ? "default.png" : user.Hinh,
                user.Id,
                user.Email,
                user.HoTen,
                user.UserName,
                user.PhoneNumber,
                user.TinhTrang,
                user.DiaChi,
                Roles = userRoles[user.Id] // Lấy từ Dictionary thay vì gọi GetRolesAsync từng lần
            });

            return Ok(userList);
        }

        #endregion


        #region Thông tin tài khoảng cùng với đơn hàng
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserByIdAndOrder(string id)
        {
            // Kiểm tra ID hợp lệ
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { Error = "ID không hợp lệ" });

            // Tìm khách hàng theo ID
            var user = await _userManager.Users
                .Include(u => u.DonDatHangs) // Include đơn hàng
                .ThenInclude(d => d.ChiTietDonDatHangs) // Include chi tiết đơn hàng
                .ThenInclude(c => c.SanPham) // Include sản phẩm
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound(new { Error = "Tài khoản không tồn tại" });

            // Lấy danh sách vai trò của khách hàng
            var roles = await _userManager.GetRolesAsync(user);

            // Chuẩn bị danh sách đơn hàng chi tiết theo UserId
            var orders = user.DonDatHangs.Select(order => new
            {
                order.MaDonHang,
                order.NgayDat,
                order.TrangThai,
                order.TongTien,
                ChiTietDonHang = order.ChiTietDonDatHangs.Select(detail => new
                {
                    detail.MaChiTiet,
                    SanPham = detail.SanPham.TenSanPham,
                    detail.SoLuong,
                    detail.Gia,
                    tongtien = detail.TongTien
                })
            });

            // Trả về thông tin khách hàng cùng với danh sách đơn hàng
            return Ok(new
            {
                Hinh = string.IsNullOrWhiteSpace(user.Hinh) ? "default.png" : user.Hinh,
                user.Id,
                user.Email,
                user.HoTen,
                user.UserName,
                user.PhoneNumber,
                user.DiaChi,
                TinhTrang = user.TinhTrang == "Hoạt Động" ? "Hoạt động" : "Bị khóa",
                Roles = roles,
                DonHang = orders
            });
        }


        #endregion

        #region Xóa Tài khoản
        /// <summary>
        /// Xóa tài khoản theo ID
        /// </summary>
        /// <response code="200">xóa thành công</response>
        /// <response code="400">Dữ liệu không hợp lệ</response>
        /// <response code="409">Email đã tồn tại</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { Error = "ID không hợp lệ" });

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { Error = "Tài khoản không tồn tại" });

            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return BadRequest(new { Error = "Tài khoản không tồn tại hoặc người dùng chưa đăng nhập" });
            }

            var roles = await _userManager.GetRolesAsync(user);

            // Kiểm tra các điều kiện trước khi xóa
            var validationResult = await ValidateDeleteUser(id, user, currentUserId, roles);
            if (validationResult != null)
                return validationResult;

            // Xóa user khỏi hệ thống
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { Error = "Xóa tài khoản thất bại", Details = result.Errors });

            return Ok(new { Message = "Xóa tài khoản thành công!" });
        }

        //Xác thực Xóa Người dùng
        private async Task<IActionResult> ValidateDeleteUser(string id, KhachHang user, string currentUserId, IList<string> roles)
        {
            // Không cho phép admin tự xóa chính mình
            if (currentUserId == id && roles.Contains("Admin"))
                return BadRequest(new { Error = "Bạn không thể tự xóa tài khoản Admin của chính mình!" });

            // Kiểm tra nếu tài khoản đang ở trạng thái "Hoạt Động"
            if (user.TinhTrang == "Hoạt Động")
                return BadRequest(new { Error = "Không thể xóa tài khoản đang hoạt động!" });

            // Kiểm tra nếu tài khoản có đơn hàng chưa xóa
            if (await HasOrders(id))
                return BadRequest(new { Error = "Không thể xóa tài khoản có đơn hàng liên kết!" });

            // Kiểm tra nếu tài khoản có hóa đơn thanh toán liên quan
            if (await HasPayments(id))
                return BadRequest(new { Error = "Không thể xóa tài khoản có hóa đơn thanh toán!" });

            return null;
        }

        // Kiểm tra tài khoản có đơn hàng hay không
        private async Task<bool> HasOrders(string userId)
        {
            return await _context.DonDatHangs.AnyAsync(d => d.UserId == userId);
        }

        // Kiểm tra tài khoản có hóa đơn thanh toán liên quan hay không
        private async Task<bool> HasPayments(string userId)
        {
            return await _context.ThanhToans
                .AnyAsync(t => _context.DonDatHangs
                .Any(d => d.MaDonHang == t.MaDonHang && d.UserId == userId));
        }

        #endregion

        #region Đổi mật khẩu
        /// <summary>
        /// Đổi mật khẩu tài khoản theo ID
        /// </summary>
        /// <response code="200">Đổi mật khẩu thành công</response>
        /// <response code="400">Dữ liệu không hợp lệ</response>
        /// <response code="409">Email đã tồn tại</response>
        [HttpPost("{id}/doi-mat-khau")]
        public async Task<IActionResult> ChangePassword(string id, [FromForm] ChangePassword_ViewModel model)
        {
            if (string.IsNullOrEmpty(id) || model == null)
                return BadRequest(new { Error = "Dữ liệu không hợp lệ" });

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { Error = "Tài khoản không tồn tại" });

            // Đổi mật khẩu
            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Code == "PasswordMismatch"))
                    return BadRequest(new { Error = "Mật khẩu cũ không đúng." });

                return BadRequest(new { Error = "Đổi mật khẩu thất bại", Details = result.Errors });
            }

            // Gửi thông báo qua email
            if (!string.IsNullOrEmpty(user.Email))
            {
                string subject = "Thông báo: Bạn đã đổi mật khẩu thành công!";
                string body = "Mật khẩu của bạn đã được thay đổi thành công. Nếu không phải bạn thực hiện, vui lòng liên hệ hỗ trợ ngay lập tức.";

                try
                {
                    _sendMail.SendEmail(user.Email, subject, body);
                }
                catch
                {
                    // Bỏ qua lỗi nếu không gửi được email
                }
            }

            return Ok(new { Message = "Đổi mật khẩu thành công!" });
        }
        #endregion

        #region Sửa thông tin tài khoản
        /// <summary>
        /// Cập nhật thông tin tài khoản theo ID
        /// </summary>      
        /// <response code="200">sửa thành công</response>
        /// <response code="400">Dữ liệu không hợp lệ</response>
        /// <response code="409">Email đã tồn tại</response>
        [HttpPut("{id}/cap-nhat-thong-tin")]
        public async Task<IActionResult> UpdateUser(string id, [FromForm] Account_ViewModel model)
        {
            if (string.IsNullOrEmpty(id) || model == null)
                return BadRequest(new { Error = "Dữ liệu không hợp lệ" });

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { Error = "Tài khoản không tồn tại" });

            if (await IsEmailTaken(user, model.Email))
                return BadRequest(new { Error = "Email này đã được sử dụng" });

            user.HoTen = model.HoTen;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.DiaChi = model.DiaChi;
            user.TinhTrang = model.TinhTrang ?? user.TinhTrang;

            if (model.HinhFile != null)
            {
                var result = await UploadUserImage(model.HinhFile);
                if (!string.IsNullOrEmpty(result.Error))
                    return BadRequest(new { result.Error });

                user.Hinh = result.FilePath;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return BadRequest(new { Error = "Cập nhật thông tin thất bại", Details = updateResult.Errors });

            return Ok(new { Message = "Cập nhật thông tin thành công!" });
        }

        //Phương thức này kiểm tra xem email có bị trùng hay không.
        private async Task<bool> IsEmailTaken(KhachHang user, string email)
        {
            if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                var existingUser = await _userManager.FindByEmailAsync(email);
                return existingUser != null && existingUser.Id != user.Id;
            }
            return false;
        }

        //Hàm này xử lý các bước kiểm tra tệp hình ảnh và tải lên hình ảnh, trả về thông báo lỗi nếu có.
        private async Task<(string? FilePath, string? Error)> UploadUserImage(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var maxSize = 5 * 1024 * 1024; // 5MB

            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
                return (null, "Định dạng tệp không hợp lệ. Chỉ hỗ trợ JPG, JPEG, PNG, GIF.");

            if (file.Length > maxSize)
                return (null, "Tệp quá lớn. Giới hạn kích thước là 5MB.");

            try
            {
                var filePath = await _fileImgUpload.UploadFileAsync(file);
                return (filePath, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi tải lên hình ảnh: {ex.Message}");
            }
        }
        #endregion

        #region Tìm kím tài khoản
        /// <summary>
        /// Tìm kiếm tài khoản theo email hoặc số điện thoại 
        /// </summary>
        [HttpGet("tim-kiem-tai-khoan")]
        public async Task<IActionResult> SearchUser([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(new { Error = "Vui lòng nhập từ khóa tìm kiếm!" });

            var users = await _userManager.Users
                .Where(u => u.Email.Contains(keyword) || u.PhoneNumber.Contains(keyword))
                .Select(user => new
                {
                    user.Id,
                    user.Email,
                    user.UserName,
                    Hinh = string.IsNullOrEmpty(user.Hinh) ? "default.png" : user.Hinh,
                    user.PhoneNumber,
                    user.TinhTrang,
                    user.DiaChi
                })
                .ToListAsync();

            if (!users.Any())
                return NotFound(new { Error = "Không tìm thấy tài khoản nào phù hợp!" });

            return Ok(users);
        }

        #endregion

        #region Danh sách vai trò người dùng chưa có
        /// <summary>
        /// Lấy danh sách tất cả quyền
        /// </summary>
        /// <response code="200">Trả về danh sách quyền</response>
        [HttpGet("{id}/vai-tro-co-the-them")]
        public async Task<IActionResult> GetAvailableRolesForUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { Error = "Tài khoản không tồn tại" });

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            // Lọc ra các vai trò mà user chưa có
            var availableRoles = allRoles.Except(userRoles).ToList();

            return Ok(availableRoles);
        }
        #endregion

        #region Thêm vai trò 
        /// <summary>
        /// Thêm vai trò cho tài khoản
        /// </summary>
        /// <response code="200">Thêm vai trò thành công</response>
        /// <response code="400">Dữ liệu không hợp lệ hoặc vai trò không tồn tại</response>
        /// <response code="404">Không tìm thấy tài khoản</response>
        [HttpPost("{id}/them-vai-tro")]
        public async Task<IActionResult> AddUserRoles(string id, [FromBody] Role_ViewModel model)
        {
            if (string.IsNullOrEmpty(id) || model.RoleNames == null || model.RoleNames.Count == 0)
                return BadRequest(new { Error = "Dữ liệu không hợp lệ" });

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { Error = "Tài khoản không tồn tại" });

            // Lấy danh sách các vai trò hiện có của người dùng
            var existingRoles = await _userManager.GetRolesAsync(user);

            foreach (var role in model.RoleNames)
            {
                if (existingRoles.Contains(role))
                    return BadRequest(new { Error = $"Người dùng đã có vai trò '{role}'" });

                if (!await _roleManager.RoleExistsAsync(role))
                    return BadRequest(new { Error = $"Vai trò '{role}' không tồn tại" });

                // Kiểm tra nếu người dùng chưa có vai trò này thì mới thêm vào
                if (!existingRoles.Contains(role))
                {
                    var result = await _userManager.AddToRoleAsync(user, role);
                    if (!result.Succeeded)
                        return BadRequest(new { Error = $"Không thể thêm vai trò '{role}'", Details = result.Errors });
                }
            }

            return Ok(new { Message = "Thêm vai trò thành công!", AssignedRoles = await _userManager.GetRolesAsync(user) });
        }
        #endregion

        #region Xóa Vai Trò
        /// <summary>
        /// Xóa vai trò của tài khoản
        /// </summary>
        /// <response code="200">Xóa vai trò thành công</response>
        /// <response code="400">Dữ liệu không hợp lệ hoặc tài khoản không thuộc vai trò này</response>
        /// <response code="404">Không tìm thấy tài khoản</response>
        [HttpDelete("{id}/xoa-vai-tro")]
        public async Task<IActionResult> RemoveUserRole(string id, [FromBody] Role_ViewModel model)
        {
            if (string.IsNullOrEmpty(id) || model.RoleNames == null || model.RoleNames.Count == 0)
                return BadRequest(new { Error = "Dữ liệu không hợp lệ" });

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { Error = "Tài khoản không tồn tại" });

            foreach (var role in model.RoleNames)
            {
                // Kiểm tra xem người dùng có thuộc vai trò cụ thể hay không
                var isInRole = await _userManager.IsInRoleAsync(user, role);

                if (!isInRole)
                    return BadRequest(new { Error = $"Tài khoản không thuộc vai trò '{role}'" });

                var result = await _userManager.RemoveFromRoleAsync(user, role);
                if (!result.Succeeded)
                    return BadRequest(new { Error = $"Xóa vai trò '{role}' thất bại", Details = result.Errors });
            }

            return Ok(new { Message = "Xóa vai trò thành công!", RemainingRoles = await _userManager.GetRolesAsync(user) });
        }
        #endregion
    }
}
