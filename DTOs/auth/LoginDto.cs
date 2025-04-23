using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.auth
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string email { get; set; } = null!;

        [Required]
        public string password { get; set; } = null!;
    }
}
