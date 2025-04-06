using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Models_Asm
{
    public class GioHang
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaGioHang { get; set; }

        [Required(ErrorMessage = "Id khách hàng không được để trống")]
        public string UserId { get; set; } //id -> UserId  // Khóa ngoại của KhachHang

        [Required(ErrorMessage = "Mã sản phẩm không được để trống")]
        public int MaSanPham { get; set; } // Khóa ngoại của SanPham

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuong { get; set; }

        [NotMapped]
        public decimal TongTien => SoLuong * SanPham.Gia;

        // Quan hệ với KhachHang
        [ForeignKey("UserId")]
        public KhachHang KhachHang { get; set; }

        // Quan hệ với SanPham
        [ForeignKey("MaSanPham")]
        public SanPham SanPham { get; set; }
    }
}
