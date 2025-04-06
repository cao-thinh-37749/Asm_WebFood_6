using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_food_Asm.Data;
using Models_Asm;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WebFood.Services;

namespace Web_food_Asm.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Home_APIController : SessionService
    {
        private readonly ConnectStr _context;

        public Home_APIController(ConnectStr context)
        {
            _context = context;
        }

        // Lấy danh sách sản phẩm có lọc và phân trang
        [HttpGet("index")]
        public async Task<IActionResult> GetProducts(int? categoryId, decimal? minPrice = null, decimal? maxPrice = null, int page = 1, int pageSize = 8)
        {
            var products = _context.SanPhams.AsQueryable();

            // Lọc theo danh mục nếu có
            if (categoryId.HasValue)
                products = products.Where(p => p.MaDanhMuc == categoryId.Value);

            // Lọc theo giá nếu có
            if (minPrice.HasValue)
                products = products.Where(p => p.Gia >= minPrice.Value);

            if (maxPrice.HasValue)
                products = products.Where(p => p.Gia <= maxPrice.Value);

            // Đếm tổng số sản phẩm
            var totalProducts = await products.CountAsync();

            // Phân trang sản phẩm
            var pagedProducts = await products.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Trả về kết quả phân trang và thông tin lọc
            return Ok(pagedProducts);
        }


        // Lấy thông tin chi tiết sản phẩm
        [HttpGet("details/{MaSanPham}")]
        public async Task<IActionResult> GetProductDetails(int MaSanPham)
        {
            var product = await _context.SanPhams.FirstOrDefaultAsync(p => p.MaSanPham == MaSanPham);
            if (product == null)
                return NotFound(new { message = "Sản phẩm không tồn tại." });

            return Ok(product);
        }

        [HttpGet("categories-dropdown")]
        public async Task<IActionResult> GetCategoriesForDropdown()
        {
            var categories = await _context.DanhMucSanPhams.ToListAsync();
            return Ok(categories);
        }
    }
}
