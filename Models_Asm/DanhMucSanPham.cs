using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Models_Asm
{
    public class DanhMucSanPham
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaDanhMuc { get; set; }

        [Required]
        public string TenDanhMuc { get; set; }

        [JsonIgnore] // Ngăn lỗi vòng lặp JSON
        public ICollection<SanPham>? SanPhams { get; set; } = new List<SanPham>(); // Khởi tạo mặc định
    }
}
