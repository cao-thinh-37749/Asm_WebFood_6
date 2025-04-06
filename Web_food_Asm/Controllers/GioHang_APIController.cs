using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using Web_food_Asm.Data;
using Models_Asm;
using WebFood.Services;

namespace Web_food_Asm.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AuthorizeUser]
    public class GioHang_APIController : SessionService
    {
        private readonly ConnectStr _context;
        private readonly SendMail _sendMail;

        public GioHang_APIController(ConnectStr context, SendMail sendMail)
        {
            _context = context;
            _sendMail = sendMail;
            _sendMail = sendMail;
        }

        [HttpGet]
        public async Task<IActionResult> GetGioHang()
        {
            var customer = await GetCurrentCustomer();
            if (customer == null) return Unauthorized("Vui lòng đăng nhập.");

            Console.WriteLine($"Email trong session: {HttpContext.Session.GetString("UserEmail")}");

            // Kiểm tra xem khách hàng có tồn tại hay không
            if (customer == null)
            {
                return Unauthorized("Không tìm thấy khách hàng.");
            }

            // Kiểm tra giỏ hàng của khách hàng
            var gioHang = await _context.GioHangs
                .Include(g => g.SanPham)
                .Where(g => g.UserId == customer.Id)
                .ToListAsync();

            // Kiểm tra nếu giỏ hàng trống
            if (gioHang.Count == 0)
            {
                Console.WriteLine("Giỏ hàng trống.");
                return Ok("Giỏ hàng của bạn hiện tại không có sản phẩm.");
            }

            // Log sản phẩm trong giỏ hàng
            foreach (var item in gioHang)
            {
                Console.WriteLine($"Sản phẩm: {item.SanPham.TenSanPham}, Số lượng: {item.SoLuong}, Giá: {item.SanPham.Gia}");
            }

            // Trả về giỏ hàng
            return Ok(gioHang);
        }



        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(int maSanPham)
        {
            var customer = await GetCurrentCustomer();
            if (customer == null) return Unauthorized("Vui lòng đăng nhập.");

            var sanPham = await _context.SanPhams.FindAsync(maSanPham);
            if (sanPham == null) return NotFound("Sản phẩm không tồn tại.");

            var gioHangItem = await _context.GioHangs.FirstOrDefaultAsync(g => g.UserId == customer.Id && g.MaSanPham == maSanPham);
            if (gioHangItem != null)
            {
                gioHangItem.SoLuong++;
            }
            else
            {
                gioHangItem = new GioHang { UserId = customer.Id, MaSanPham = maSanPham, SoLuong = 1 };
                await _context.GioHangs.AddAsync(gioHangItem);
            }
            //gioHangItem.TongTien = gioHangItem.SoLuong * sanPham.Gia;
            await _context.SaveChangesAsync();

            return Ok("Sản phẩm đã được thêm vào giỏ hàng.");
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteFromCart(int maSanPham)
        {
            var customer = await GetCurrentCustomer();
            if (customer == null) return Unauthorized("Vui lòng đăng nhập.");

            var gioHangItem = await _context.GioHangs.FirstOrDefaultAsync(g => g.UserId == customer.Id && g.MaSanPham == maSanPham);
            if (gioHangItem == null) return NotFound("Sản phẩm không tồn tại trong giỏ hàng.");

            _context.GioHangs.Remove(gioHangItem);
            await _context.SaveChangesAsync();
            return Ok("Sản phẩm đã xóa khỏi giỏ hàng.");
        }

        [HttpPut("increase")]
        public async Task<IActionResult> IncreaseQuantity(int maSanPham)
        {
            return await UpdateQuantity(maSanPham, 1);
        }

        [HttpPut("decrease")]
        public async Task<IActionResult> DecreaseQuantity(int maSanPham)
        {
            return await UpdateQuantity(maSanPham, -1);
        }

        private async Task<IActionResult> UpdateQuantity(int maSanPham, int quantityChange)
        {
            var customer = await GetCurrentCustomer();
            if (customer == null) return Unauthorized("Vui lòng đăng nhập.");

            var gioHangItem = await _context.GioHangs.FirstOrDefaultAsync(g => g.UserId == customer.Id && g.MaSanPham == maSanPham);
            if (gioHangItem == null) return NotFound("Sản phẩm không tồn tại trong giỏ hàng.");

            gioHangItem.SoLuong += quantityChange;
            if (gioHangItem.SoLuong <= 0)
            {
                _context.GioHangs.Remove(gioHangItem);
            }
            else
            {
                var sanPham = await _context.SanPhams.FindAsync(maSanPham);
                //gioHangItem.TongTien = gioHangItem.SoLuong * sanPham.Gia;
            }

            await _context.SaveChangesAsync();
            return Ok("Cập nhật số lượng thành công.");
        }

        private async Task<KhachHang> GetCurrentCustomer()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail)) return null;

            return await _context.KhachHangs.FirstOrDefaultAsync(k => k.Email == userEmail);
        }

        #region Thanh Toán Qua VNPAY

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromQuery] string paymentMethod, [FromBody] string otp)
        {
            var customer = await GetCurrentCustomer();
            if (customer == null) return Unauthorized("Vui lòng đăng nhập.");

            // Lấy giỏ hàng của khách hàng
            var gioHang = await _context.GioHangs
                .Include(g => g.SanPham)
                .Where(g => g.UserId == customer.Id)
                .ToListAsync();

            if (!gioHang.Any()) return BadRequest("Giỏ hàng trống.");

            // Tính tổng tiền đơn hàng
            decimal tongTien = gioHang.Sum(g => g.SanPham.Gia * g.SoLuong);

            // Tạo đơn đặt hàng
            var donHang = new DonDatHang
            {
                UserId = customer.Id,
                NgayDat = DateTime.Now,
                TrangThai = "Đang xử lý",
                TongTien = tongTien
            };

            _context.DonDatHangs.Add(donHang);
            await _context.SaveChangesAsync();

            // Kiểm tra mã OTP (giả sử OTP đã được gửi qua email hoặc SMS)
            if (string.IsNullOrEmpty(otp) || otp != "123456") // Kiểm tra mã OTP giả
            {
                return BadRequest("Mã OTP không hợp lệ.");
            }

            // Chọn phương thức thanh toán
            if (paymentMethod.ToLower() == "vnpay")
            {
                // Tạo link thanh toán VNPAY
                string paymentUrl = VnPayHelper.CreatePaymentUrl(donHang.MaDonHang, tongTien);
                return Ok(new { Message = "Vui lòng thanh toán qua VNPAY", PaymentUrl = paymentUrl });
            }
            else
            {
                // Xử lý thanh toán MoMo (Giữ nguyên)
                var thanhToan = new ThanhToan
                {
                    MaDonHang = donHang.MaDonHang,
                    SoTien = tongTien,
                    PhuongThuc = "MoMo",
                    TrangThai = "Chưa thanh toán", // Trạng thái mặc định
                    NgayThanhToan = null
                };

                _context.ThanhToans.Add(thanhToan);
                await _context.SaveChangesAsync();

                // Xóa giỏ hàng sau khi đặt hàng thành công
                _context.GioHangs.RemoveRange(gioHang);
                await _context.SaveChangesAsync();

                // Gửi email xác nhận đơn hàng
                _sendMail.SendEmail(customer.Email,
                                    $"Xác nhận đơn hàng #{donHang.MaDonHang}",
                                    $"Đơn hàng {donHang.MaDonHang} đã thanh toán thành công. Cảm ơn bạn đã mua sắm!");

                return Ok(new { Message = "Đặt hàng thành công!", DonHangId = donHang.MaDonHang });
            }
        }
        #endregion
    }
}


