using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Models_Asm
{
    public class ThanhToan
    {
        // MaDonHang vừa là khóa chính vừa là khóa ngoại
        [Key, ForeignKey("DonDatHang")]
        public int MaDonHang { get; set; }

        // Kiểm tra hợp lệ cho SoTien
        [Required(ErrorMessage = "Số tiền không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn hoặc bằng 0")]
        public decimal SoTien { get; set; }

        // Phương thức thanh toán, mặc định là MoMo
        [Required(ErrorMessage = "Phương thức thanh toán không được để trống")]
        [StringLength(50, ErrorMessage = "Phương thức thanh toán tối đa 50 ký tự")]
        public string PhuongThuc { get; set; } = "MoMo";

        // Trạng thái thanh toán, mặc định là Chưa thanh toán
        [Required(ErrorMessage = "Trạng thái thanh toán không được để trống")]
        [StringLength(50, ErrorMessage = "Trạng thái thanh toán tối đa 50 ký tự")]
        public string TrangThai { get; set; } = "Chưa thanh toán";

        // Ngày thanh toán, có thể để null nếu chưa thanh toán
        public DateTime? NgayThanhToan { get; set; }

        // Mối quan hệ với bảng DonDatHang
        public DonDatHang DonDatHang { get; set; }
    }
}
