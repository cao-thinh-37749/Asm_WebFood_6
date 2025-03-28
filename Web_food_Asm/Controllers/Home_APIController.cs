using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_food_Asm.Data;
using Web_food_Asm.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;

namespace Web_food_Asm.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Home_APIController : ControllerBase
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

        // Lấy danh sách sản phẩm yêu thích của người dùng
        [HttpGet("favorites")]
        public async Task<IActionResult> GetFavoriteProducts()
        {
            var customer = await GetCustomerAsync(HttpContext);
            if (customer == null)
                return Unauthorized(new { message = "Người dùng chưa đăng nhập." });

            var favoriteProducts = await _context.SanPhamYeuThichs
                .Where(y => y.UserId == customer.Id)
                .Include(y => y.SanPham)
                .Select(y => y.SanPham)
                .ToListAsync();

            return Ok(favoriteProducts);
        }

        // Thêm sản phẩm vào danh sách yêu thích
        [HttpPost("ThemYeuThich")]
        [HttpPost]
        public async Task<IActionResult> AddToFavorite(int maSanPham)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Người dùng chưa đăng nhập.");
            }

            var favoriteItem = new SanPhamYeuThich
            {
                UserId = userId,
                MaSanPham = maSanPham
            };

            _context.SanPhamYeuThichs.Add(favoriteItem);
            await _context.SaveChangesAsync();

            return Ok("Đã thêm vào danh sách yêu thích.");
        }



        // Xóa sản phẩm khỏi danh sách yêu thích
        [HttpDelete("XoaYeuThich/{maSanPham}")]
        public async Task<IActionResult> RemoveFromFavorite(int maSanPham)
        {
            var customer = await GetCustomerAsync(HttpContext);
            if (customer == null)
                return Unauthorized(new { message = "Người dùng chưa đăng nhập." });

            var favorite = await _context.SanPhamYeuThichs.FirstOrDefaultAsync(y => y.UserId== customer.Id && y.MaSanPham == maSanPham);
            if (favorite == null)
                return NotFound(new { message = "Sản phẩm không tồn tại trong danh sách yêu thích." });

            _context.SanPhamYeuThichs.Remove(favorite);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Sản phẩm đã được xóa khỏi danh sách yêu thích." });
        }

        // Chính sách bảo mật
        [HttpGet("privacy")]
        public IActionResult GetPrivacyPolicy()
        {
            return Ok(new { message = "Chính sách bảo mật của cửa hàng." });
        }

        // Xử lý lỗi
        [HttpGet("error")]
        public IActionResult HandleError()
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Có lỗi xảy ra." });
        }

        // Lấy thông tin khách hàng từ session
        private async Task<KhachHang?> GetCustomerAsync(HttpContext httpContext)
        {
            var userEmail = httpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail)) return null;

            return await _context.KhachHangs.FirstOrDefaultAsync(k => k.Email == userEmail);
        }
    }
}
