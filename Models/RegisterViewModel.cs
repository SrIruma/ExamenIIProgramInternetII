using System.ComponentModel.DataAnnotations;

namespace ExamenII.Models
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        public bool IsAdmin { get; set; } = false;
    }
}