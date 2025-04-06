using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Models_Asm
{
    public class DonDatHang
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaDonHang { get; set; }

        [Required(ErrorMessage = "Id khách hàng không được để trống")]
        public string UserId { get; set; } // Khóa ngoại của KhachHang

        [Required(ErrorMessage = "Ngày đặt không được để trống")]
        public DateTime NgayDat { get; set; } = DateTime.Now;

        [StringLength(50, ErrorMessage = "Trạng thái đơn hàng tối đa 50 ký tự")]
        public string TrangThai { get; set; } = "Đang xử lý";

        [Required(ErrorMessage = "Tổng tiền không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tổng tiền phải lớn hơn 0")]
        public decimal TongTien { get; set; }

        // Quan hệ với KhachHang
        [ForeignKey("UserId")]
        public KhachHang KhachHang { get; set; }

        // Quan hệ với ChiTietDonDatHang
        public ICollection<ChiTietDonDatHang> ChiTietDonDatHangs { get; set; }

        // Quan hệ với ThanhToan
        public ThanhToan ThanhToan { get; set; }
    }
}
