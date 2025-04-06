using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Models_Asm
{
    public class ChiTietDonDatHang
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaChiTiet { get; set; }

        [Required(ErrorMessage = "Mã đơn hàng không được để trống")]
        public int MaDonHang { get; set; } // Khóa ngoại của DonDatHang

        [Required(ErrorMessage = "Mã sản phẩm không được để trống")]
        public int MaSanPham { get; set; } // Khóa ngoại của SanPham

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuong { get; set; }

        [Required(ErrorMessage = "Giá không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Gia { get; set; }

        public DateTime? NgayGiao { get; set; }
        public DateTime? NgayNhan { get; set; }
        public DateTime? NgayThanhToan { get; set; }

        [StringLength(50, ErrorMessage = "Trạng thái tối đa 50 ký tự")]
        public string TrangThai { get; set; } = "Đang xử lý";

        [NotMapped]
        public decimal TongTien => SoLuong * Gia;

        [ForeignKey("UserId")]
        public virtual KhachHang? KhachHang { get; set; } // Điều hướng đến KhachHang

        [ForeignKey("MaDonHang")]
        public DonDatHang DonDatHang { get; set; }

        [ForeignKey("MaSanPham")]
        public SanPham SanPham { get; set; }
    }
}
