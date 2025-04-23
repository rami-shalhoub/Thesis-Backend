using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.auth
{
    public class RefreshTokenDto
    {
        [Required]
        public string refreshToken { get; set; } = null!;
    }
}
