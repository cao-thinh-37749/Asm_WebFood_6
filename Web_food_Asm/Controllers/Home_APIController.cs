using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_food_Asm.Data;
using Web_food_Asm.Models;
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

            if (categoryId.HasValue)
                products = products.Where(p => p.MaDanhMuc == categoryId.Value);

            if (minPrice.HasValue && maxPrice.HasValue)
                products = products.Where(p => p.Gia >= minPrice.Value && p.Gia <= maxPrice.Value);

            var totalProducts = await products.CountAsync();
            var pagedProducts = await products.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new
            {
                TotalProducts = totalProducts,
                CurrentCategoryId = categoryId,
                CurrentMinPrice = minPrice,
                CurrentMaxPrice = maxPrice,
                CurrentPage = page,
                Products = pagedProducts
            });
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
    }
}
