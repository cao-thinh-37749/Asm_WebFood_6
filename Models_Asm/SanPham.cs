using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Models_Asm
{
    public class SanPham
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaSanPham { get; set; }

        // Tên sản phẩm yêu cầu không được trống và dài tối đa 100 ký tự
        [Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm không được vượt quá 100 ký tự.")]
        public string TenSanPham { get; set; }

        // Hình ảnh yêu cầu không được trống và dài tối đa 255 ký tự
        [Required(ErrorMessage = "Hình ảnh không được để trống.")]
        [StringLength(255, ErrorMessage = "Đường dẫn hình ảnh không được vượt quá 255 ký tự.")]
        public string HinhAnh { get; set; }

        // Mô tả sản phẩm, có thể để trống nhưng không vượt quá 500 ký tự
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string MoTa { get; set; }

        // Giá phải là một số dương
        [Required(ErrorMessage = "Giá sản phẩm không được để trống.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0.")]
        public decimal Gia { get; set; }

        // Số lượng phải là số dương
        [Required(ErrorMessage = "Số lượng sản phẩm không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng sản phẩm phải lớn hơn 0.")]
        public int SoLuong { get; set; }

        // Mã danh mục sản phẩm là khóa ngoại, không được để trống
        [Required(ErrorMessage = "Danh mục sản phẩm không được để trống.")]
        public int MaDanhMuc { get; set; }

        // Ngày tạo và ngày cập nhật sản phẩm
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime NgayCapNhat { get; set; } = DateTime.Now;

        // Quan hệ với bảng DanhMucSanPham
        [ForeignKey("MaDanhMuc")]
        public DanhMucSanPham DanhMucSanPham { get; set; }
    }
}
