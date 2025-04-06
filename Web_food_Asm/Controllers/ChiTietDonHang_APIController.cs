using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using Web_food_Asm.Data;
using Models_Asm;
using WebFood.Services;

namespace Web_food_Asm.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AuthorizeUser]
    public class ChiTietDonHang_APIController : SessionService
    {
        private readonly ConnectStr _connectStr;

        public ChiTietDonHang_APIController(ConnectStr connectStr)
        {
            _connectStr = connectStr;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetOrderDetailsList()
        {
            var chiTietDonHangs = await _connectStr.ChiTietDonDatHangs
                .Include(ct => ct.KhachHang)
                .Include(ct => ct.SanPham)
                .ToListAsync();

            return Ok(chiTietDonHangs);
        }

        [HttpGet("details/{maChiTiet}")]
        public async Task<IActionResult> GetOrderDetails(int maChiTiet)
        {
            var chiTiet = await _connectStr.ChiTietDonDatHangs
                .Include(ct => ct.KhachHang)
                .Include(ct => ct.SanPham)
                .FirstOrDefaultAsync(ct => ct.MaChiTiet == maChiTiet);

            if (chiTiet == null)
                return NotFound(new { message = "Không tìm thấy chi tiết đơn hàng." });

            return Ok(chiTiet);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrderDetail([FromBody] ChiTietDonDatHang chiTiet)
        {
            if (chiTiet == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ." });

            _connectStr.ChiTietDonDatHangs.Add(chiTiet);
            await _connectStr.SaveChangesAsync();

            return Ok(new { message = "Chi tiết đơn hàng đã được tạo thành công." });
        }

        [HttpPut("update/{maChiTiet}")]
        public async Task<IActionResult> UpdateOrderDetail(int maChiTiet, [FromBody] ChiTietDonDatHang updatedChiTiet)
        {
            var chiTiet = await _connectStr.ChiTietDonDatHangs.FindAsync(maChiTiet);
            if (chiTiet == null)
                return NotFound(new { message = "Không tìm thấy chi tiết đơn hàng." });

            chiTiet.SoLuong = updatedChiTiet.SoLuong;
            chiTiet.Gia = updatedChiTiet.Gia;
            chiTiet.TrangThai = updatedChiTiet.TrangThai;
            await _connectStr.SaveChangesAsync();

            return Ok(new { message = "Chi tiết đơn hàng đã được cập nhật." });
        }

        [HttpDelete("delete/{maChiTiet}")]
        public async Task<IActionResult> DeleteOrderDetail(int maChiTiet)
        {
            var chiTiet = await _connectStr.ChiTietDonDatHangs.FindAsync(maChiTiet);
            if (chiTiet == null)
                return NotFound(new { message = "Không tìm thấy chi tiết đơn hàng." });

            _connectStr.ChiTietDonDatHangs.Remove(chiTiet);
            await _connectStr.SaveChangesAsync();

            return Ok(new { message = "Chi tiết đơn hàng đã được xóa thành công." });
        }
    }
}