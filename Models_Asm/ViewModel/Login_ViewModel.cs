using System.ComponentModel.DataAnnotations;

namespace Models_Asm
{
    public class Login_ViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
