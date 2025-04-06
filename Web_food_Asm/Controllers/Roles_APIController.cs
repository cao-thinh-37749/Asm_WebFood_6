using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models_Asm;
using WebFood.Services;

namespace Web_food_Asm.Controllers
{
    [ApiController]
    [Route("api/roles")]
    [AuthorizeUser]
    public class Roles_APIController : SessionService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<KhachHang> _userManager;

        public Roles_APIController(RoleManager<IdentityRole> roleManager, UserManager<KhachHang> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        #region Lấy danh sách quyền
        /// <summary>
        /// Lấy danh sách tất cả quyền
        /// </summary>
        /// <response code="200">Trả về danh sách quyền</response>
        [HttpGet("danh-sach")]
        public IActionResult GetAllRoles()
        {
            var roles = _roleManager.Roles.Select(r => new { r.Id, r.Name }).ToList();
            return Ok(roles);
        }
        #endregion

        #region Lấy quyền theo ID
        /// <summary>
        /// Lấy quyền theo ID và danh sách người dùng trong vai trò đó
        /// </summary>
        /// <response code="200">Trả về thông tin quyền và danh sách người dùng</response>
        /// <response code="400">ID không hợp lệ</response>
        /// <response code="404">Không tìm thấy quyền</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { Error = "ID không hợp lệ" });

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound(new { Error = "Quyền không tồn tại" });

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            return Ok(new
            {
                role.Id,
                role.Name,
                TotalAccounts = usersInRole.Count, 
                Users = usersInRole.Select(u => new
                {
                    u.Hinh,
                    u.UserName,
                    u.Email,
                    u.PhoneNumber,
                    u.DiaChi,
                    u.TinhTrang
                })
            });
        }
        #endregion

        #region Thêm quyền mới
        /// <summary>
        /// Thêm mới một quyền
        /// </summary>
        /// <response code="201">Thêm quyền thành công</response>
        /// <response code="400">Tên quyền không hợp lệ</response>
        /// <response code="409">Quyền đã tồn tại</response>
        [HttpPost("them-quyen")]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                return BadRequest(new { Error = "Tên quyền không được để trống" });

            // Kiểm tra độ dài của tên quyền
            if (roleName.Length < 3 || roleName.Length > 50)
                return BadRequest(new { Error = "Tên quyền phải có độ dài từ 3 đến 50 ký tự" });

            var existingRole = await _roleManager.FindByNameAsync(roleName);
            if (existingRole != null)
                return BadRequest(new { Error = "Quyền này đã tồn tại" });

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
                return BadRequest(new { Error = "Thêm quyền thất bại", Details = result.Errors });

            return CreatedAtAction(nameof(GetAllRoles), new { }, new { Message = "Thêm quyền thành công!" });
        }

        #endregion

        #region Cập nhật quyền
        /// <summary>
        /// Cập nhật tên quyền theo ID
        /// </summary>
        /// <response code="200">Cập nhật thành công</response>
        /// <response code="400">Dữ liệu không hợp lệ</response>
        /// <response code="404">Không tìm thấy quyền</response>
        /// <response code="409">Tên quyền mới đã tồn tại</response>
        [HttpPut("{id}/cap-nhat-quyen")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] string newName)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(newName))
                return BadRequest(new { Error = "Dữ liệu không hợp lệ" });

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound(new { Error = "Quyền không tồn tại" });

            // Kiểm tra nếu quyền mới đã tồn tại
            var existingRole = await _roleManager.FindByNameAsync(newName);
            if (existingRole != null)
                return BadRequest(new { Error = "Tên quyền mới đã tồn tại" });

            role.Name = newName;
            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                // Chi tiết lỗi sẽ được trả về nếu có
                return BadRequest(new { Error = "Cập nhật quyền thất bại", Details = result.Errors.Select(e => e.Description) });
            }

            return Ok(new { Message = "Cập nhật quyền thành công!", Role = newName });
        }

        #endregion

        #region Xóa quyền
        /// <summary>
        /// Xóa quyền theo ID
        /// </summary>
        /// <response code="200">Xóa quyền thành công</response>
        /// <response code="400">ID không hợp lệ</response>
        /// <response code="404">Không tìm thấy quyền</response>
        /// <response code="409">Không thể xóa quyền vì có tài khoản đang sử dụng</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { Error = "ID không hợp lệ" });

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return BadRequest(new { Error = "Quyền không tồn tại" });

            // Kiểm tra nếu có tài khoản đang sử dụng quyền này
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            if (usersInRole.Any())
                return BadRequest(new { Error = "Không thể xóa quyền vì có tài khoản đang sử dụng", UsersCount = usersInRole.Count });

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
                return BadRequest(new { Error = "Xóa quyền thất bại", Details = result.Errors.Select(e => e.Description) });

            return Ok(new { Message = "Xóa quyền thành công!" });
        }
        #endregion
    }
}
