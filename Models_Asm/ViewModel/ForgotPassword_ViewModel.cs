using System.ComponentModel.DataAnnotations;

namespace Models_Asm
{
    public class ForgotPassword_ViewModel
    {
        [Required]
        public string Email { get; set; } 
    }
}
