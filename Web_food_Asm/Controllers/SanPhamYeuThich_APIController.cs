using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_food_Asm.Data;
using Web_food_Asm.Models;
using WebFood.Services;

[Route("api/yeuthich")]
[ApiController]
[AuthorizeUser] // Tự động kiểm tra đăng nhập
public class SanPhamYeuThichController : SessionService
{
    private readonly ConnectStr _context;
    public SanPhamYeuThichController(ConnectStr context)
    {
        _context = context;
    }

    #region Lấy danh sách sản phẩm yêu thích
    /// <summary>
    /// Lấy danh sách sản phẩm yêu thích của người dùng
    /// </summary>
    /// <returns>Danh sách sản phẩm yêu thích</returns>
    [HttpGet("danh-sach-yeu-thich")]
    public async Task<IActionResult> GetFavorites()
    {
        var favoriteProducts = await _context.SanPhamYeuThichs
            .Where(y => y.UserId == UserId)
            .Include(y => y.SanPham)
            .Select(y => y.SanPham)
            .ToListAsync();

        return Ok(favoriteProducts);
    }
    #endregion

    #region Thêm sản phẩm vào danh sách yêu thích
    /// <summary>
    /// Thêm sản phẩm vào danh sách yêu thích
    /// </summary>
    /// <param name="maSanPham">Mã sản phẩm cần thêm</param>
    /// <returns>Thông báo kết quả</returns>
    [HttpPost("them-yeu-thich/{maSanPham}")]
    public async Task<IActionResult> AddToFavorite(int maSanPham)
    {
        if (!await _context.SanPhams.AnyAsync(p => p.MaSanPham == maSanPham))
            return NotFound(new { message = "Sản phẩm không tồn tại." });

        if (await _context.SanPhamYeuThichs.AnyAsync(y => y.UserId == UserId && y.MaSanPham == maSanPham))
            return BadRequest(new { message = "Sản phẩm đã có trong danh sách yêu thích." });

        await _context.SanPhamYeuThichs.AddAsync(new SanPhamYeuThich { UserId = UserId, MaSanPham = maSanPham });
        await _context.SaveChangesAsync();

        return Ok(new { message = "Đã thêm vào danh sách yêu thích." });
    }
    #endregion

    #region Xóa sản phẩm khỏi danh sách yêu thích
    /// <summary>
    /// Xóa sản phẩm khỏi danh sách yêu thích
    /// </summary>
    /// <param name="maSanPham">Mã sản phẩm cần xóa</param>
    /// <returns>Thông báo kết quả</returns>
    [HttpDelete("xoa-san-pham/{maSanPham}")]
    public async Task<IActionResult> DeleteFavorite(int maSanPham)
    {
        var deleteFavorite = await _context.SanPhamYeuThichs
            .FirstOrDefaultAsync(y => y.UserId == UserId && y.MaSanPham == maSanPham);

        if (deleteFavorite == null)
            return NotFound(new { message = "Sản phẩm không tồn tại trong danh sách yêu thích." });

        _context.SanPhamYeuThichs.Remove(deleteFavorite);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Sản phẩm đã được xóa khỏi danh sách yêu thích." });
    }
    #endregion
}