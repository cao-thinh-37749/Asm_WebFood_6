using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models_Asm
{
    public class Register_ViewModel
    {
        [Required(ErrorMessage = "Địa chỉ không được để trống.")]
        [StringLength(400, ErrorMessage = "Đường dẫn hình ảnh không được vượt quá 400 ký tự.")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }


        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được quá 255 ký tự")]
        public string DiaChi { get; set; }
    }
}
