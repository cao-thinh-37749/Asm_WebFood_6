using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Web_food_Asm.Models
{
    public class SanPhamYeuThich
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Tự động tăng
        public int MaYeuThich { get; set; }  // Khóa chính, tự động sinh giá trị

        [Required(ErrorMessage = "Id khách hàng không được để trống.")]
        public string UserId { get; set; }  // Khóa ngoại của KhachHang

        [Required(ErrorMessage = "Mã sản phẩm không được để trống.")]
        public int MaSanPham { get; set; }  // Khóa ngoại của SanPham

        // Mối quan hệ với bảng KhachHang
        [ForeignKey("UserId")]
        public KhachHang KhachHang { get; set; }

        // Mối quan hệ với bảng SanPham
        [ForeignKey("MaSanPham")]
        public SanPham SanPham { get; set; }
    }
}


