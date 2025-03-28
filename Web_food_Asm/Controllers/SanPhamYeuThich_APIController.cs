using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Web_food_Asm.Data;
using Web_food_Asm.Models;

namespace Web_food_Asm.Controllers
{
    [Route("api/yeuthich")]
    [ApiController]
    [Authorize] // Chỉ cho phép người dùng đã đăng nhập sử dụng API
        public class SanPhamYeuThichController : ControllerBase
        {
            private readonly ConnectStr _context;

            public SanPhamYeuThichController(ConnectStr context)
            {
                _context = context;
            }

            // Kiểm tra session và lấy khách hàng
            private KhachHang? GetCustomerEmail()
            {
                var userEmail = HttpContext.Session.GetString("UserEmail");
                return string.IsNullOrEmpty(userEmail) ? null : _context.KhachHangs.FirstOrDefault(k => k.Email == userEmail);
            }

            // Kiểm tra xem người dùng đã đăng nhập chưa
            private bool IsUserLoggedIn() => GetCustomerEmail() != null;

            /// <summary>
            /// Lấy danh sách sản phẩm yêu thích của người dùng
            /// </summary>
            [HttpGet]
            public IActionResult GetFavorites()
            {
                var customer = GetCustomerEmail();
                if (!IsUserLoggedIn() || customer == null)
                    return Unauthorized(new { message = "Bạn cần đăng nhập để xem danh sách yêu thích." });

                var favoriteProducts = _context.SanPhamYeuThichs
                    .Include(y => y.SanPham)
                    .Where(y => y.UserId == customer.Id)
                    .Select(y => new { y.SanPham.MaSanPham, y.SanPham.TenSanPham, y.SanPham.Gia })
                    .ToList();

                return Ok(new { favoriteCount = favoriteProducts.Count, favorites = favoriteProducts });
            }

            /// <summary>
            /// Thêm sản phẩm vào danh sách yêu thích
            /// </summary>
            [HttpPost("add")]
            public IActionResult AddToFavorites([FromBody] int maSanPham)
            {
                var customer = GetCustomerEmail();

                if (!IsUserLoggedIn() || customer == null)
                    return Unauthorized(new { message = "Bạn cần đăng nhập để thêm sản phẩm yêu thích." });

                if (_context.SanPhamYeuThichs.Any(y => y.UserId == customer.Id && y.MaSanPham == maSanPham))
                    return BadRequest(new { message = "Sản phẩm đã có trong danh sách yêu thích." });

                _context.SanPhamYeuThichs.Add(new SanPhamYeuThich
                {
                    UserId = customer.Id,
                    MaSanPham = maSanPham
                });
                _context.SaveChanges();

                return Ok(new { message = "Thêm sản phẩm yêu thích thành công!" });
            }

            /// <summary>
            /// Xóa sản phẩm khỏi danh sách yêu thích
            /// </summary>
            [HttpDelete("delete/{maSanPham}")]
            public IActionResult DeleteFavorite(int maSanPham)
            {
                var customer = GetCustomerEmail();
                if (!IsUserLoggedIn() || customer == null)
                    return Unauthorized(new { message = "Bạn cần đăng nhập để xóa sản phẩm yêu thích." });

                var deleteFavorite = _context.SanPhamYeuThichs
                    .FirstOrDefault(y => y.UserId == customer.Id && y.MaSanPham == maSanPham);

                if (deleteFavorite == null)
                    return NotFound(new { message = "Sản phẩm không tồn tại trong danh sách yêu thích." });

                _context.SanPhamYeuThichs.Remove(deleteFavorite);
                _context.SaveChanges();

                return Ok(new { message = "Sản phẩm đã được xóa khỏi danh sách yêu thích." });
            }
        }
    }

