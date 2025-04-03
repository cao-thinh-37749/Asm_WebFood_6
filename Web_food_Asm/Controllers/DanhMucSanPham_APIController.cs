using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web_food_Asm.Data;
using Web_food_Asm.Models;
using WebFood.Services;

namespace Web_food_Asm.Controllers
{
    [Route("api/admin/danh-muc")]
    [ApiController]
    [AuthorizeUser]
    public class DanhMucSanPhamController : SessionService
    {
        private readonly ConnectStr _context;

        public DanhMucSanPhamController(ConnectStr context)
        {
            _context = context;
        }

        #region Lấy danh sách danh mục sản phẩm
        /// <summary>
        /// Lấy toàn bộ danh sách danh mục sản phẩm, bao gồm danh sách sản phẩm liên quan.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _context.DanhMucSanPhams.ToListAsync();
            return Ok(categories);
        }

        /// <summary>
        /// Lấy danh mục sản phẩm theo ID.
        /// </summary>
        /// <param name="id">ID của danh mục cần tìm</param>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _context.DanhMucSanPhams
                .Include(c => c.SanPhams)
                .FirstOrDefaultAsync(c => c.MaDanhMuc == id);

            if (category == null) return NotFound("Không tìm thấy danh mục.");
            return Ok(category);
        }
        #endregion

        #region Thêm DMSP
        /// <summary>
        /// Thêm mới một danh mục sản phẩm.
        /// </summary>
        /// <param name="danhMucSanPham">Thông tin danh mục cần thêm</param>
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] DanhMucSanPham danhMucSanPham)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(danhMucSanPham.TenDanhMuc))
                return BadRequest("Tên danh mục không được để trống.");

            // Kiểm tra danh mục đã tồn tại chưa
            var existingCategory = await _context.DanhMucSanPhams
                .FirstOrDefaultAsync(c => c.TenDanhMuc.ToLower() == danhMucSanPham.TenDanhMuc.ToLower());

            if (existingCategory != null)
                return BadRequest("Danh mục này đã tồn tại!");

            // Đảm bảo danh mục mới không chứa sản phẩm khi tạo
            danhMucSanPham.SanPhams = new List<SanPham>();

            _context.DanhMucSanPhams.Add(danhMucSanPham);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoryById), new { id = danhMucSanPham.MaDanhMuc }, danhMucSanPham);
        }
        #endregion

        #region Chỉnh Sửa Danh Mục
        /// <summary>
        /// Cập nhật thông tin danh mục sản phẩm.
        /// </summary>
        /// <param name="id">ID của danh mục cần cập nhật</param>
        /// <param name="danhMucSanPham">Thông tin mới của danh mục</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] DanhMucSanPham danhMucSanPham)
        {
            if (id != danhMucSanPham.MaDanhMuc) return BadRequest("ID không khớp.");

            var category = await _context.DanhMucSanPhams.FindAsync(id);
            if (category == null) return NotFound("Không tìm thấy danh mục.");

            // Cập nhật thông tin danh mục
            category.TenDanhMuc = danhMucSanPham.TenDanhMuc;

            _context.DanhMucSanPhams.Update(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        #endregion

        #region Xóa DMSP
        /// <summary>
        /// Xóa danh mục sản phẩm (chỉ khi không có sản phẩm liên quan).
        /// </summary>
        /// <param name="id">ID của danh mục cần xóa</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.DanhMucSanPhams
                .Include(c => c.SanPhams)
                .FirstOrDefaultAsync(c => c.MaDanhMuc == id);

            if (category == null) return NotFound("Không tìm thấy danh mục.");

            // Không cho phép xóa nếu danh mục đang chứa sản phẩm
            if (category.SanPhams.Any())
                return BadRequest("Không thể xóa danh mục khi còn sản phẩm liên quan.");

            _context.DanhMucSanPhams.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        #endregion
    }
}