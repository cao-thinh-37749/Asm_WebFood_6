using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Web_food_Asm.Data;
using Web_food_Asm.Models;

namespace Web_food_Asm.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonHang_APIController : ControllerBase
    {
        private readonly ConnectStr _connectStr;
        private readonly SendMail _sendMail;  // Giả sử bạn có một dịch vụ gửi mail

        public DonHang_APIController(ConnectStr connectStr, SendMail sendMail)
        {
            _connectStr = connectStr;
            _sendMail = sendMail;
        }

        // API nhận kết quả thanh toán từ VNPAY/MoMo
        [HttpPost("payment-result")]
        public async Task<IActionResult> PaymentResult([FromBody] PaymentResult paymentResult)
        {
            // Kiểm tra kết quả thanh toán
            bool isPaymentSuccessful = paymentResult.IsSuccess;  // Kết quả thanh toán (true nếu thành công)

            // Lấy thông tin đơn hàng dựa trên mã đơn hàng từ paymentResult
            var donHang = await _connectStr.DonDatHangs
                .Include(dh => dh.ThanhToan)
                .FirstOrDefaultAsync(dh => dh.MaDonHang == paymentResult.MaDonHang);

            if (donHang == null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

            // Cập nhật trạng thái thanh toán cho đơn hàng
            if (isPaymentSuccessful)
            {
                // Cập nhật trạng thái đơn hàng thành "Đã thanh toán"
                donHang.TrangThai = "Đã thanh toán";

                // Cập nhật thông tin thanh toán vào bảng ThanhToan
                donHang.ThanhToan.TrangThai = "Đã thanh toán";
                donHang.ThanhToan.NgayThanhToan = DateTime.Now;

                // Lưu thay đổi vào cơ sở dữ liệu
                _connectStr.DonDatHangs.Update(donHang);
                _connectStr.ThanhToans.Update(donHang.ThanhToan);
                await _connectStr.SaveChangesAsync();

                // Xóa giỏ hàng của khách hàng sau khi thanh toán thành công
                var gioHang = await _connectStr.GioHangs
                    .Where(g => g.UserId == donHang.UserId)
                    .ToListAsync();

                _connectStr.GioHangs.RemoveRange(gioHang);
                await _connectStr.SaveChangesAsync();

                // Gửi email xác nhận đơn hàng
                var customer = await _connectStr.KhachHangs.FirstOrDefaultAsync(kh => kh.Id == donHang.UserId);
                if (customer != null)
                {
                    _sendMail.SendEmail(customer.Email,
                        $"Xác nhận đơn hàng #{donHang.MaDonHang}",
                        $"Đơn hàng #{donHang.MaDonHang} đã thanh toán thành công. Cảm ơn bạn đã mua sắm!");
                }

                return Ok(new { Message = "Cập nhật trạng thái thanh toán thành công.", DonHangId = donHang.MaDonHang });
            }
            else
            {
                // Nếu thanh toán thất bại, bạn có thể xử lý thông báo lỗi
                donHang.TrangThai = "Thanh toán thất bại";
                _connectStr.DonDatHangs.Update(donHang);
                await _connectStr.SaveChangesAsync();

                return BadRequest(new { Message = "Thanh toán thất bại. Vui lòng thử lại." });
            }
        }
    }
}
