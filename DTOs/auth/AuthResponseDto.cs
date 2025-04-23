using System;

namespace Backend.DTOs.auth
{
    public class AuthResponseDto
    {
        public Guid userID { get; set; }
        public string name { get; set; } = null!;
        public string email { get; set; } = null!;
        public string role { get; set; } = null!;
        public string organisationID { get; set; } = null!;
        public string accessToken { get; set; } = null!;
        public string refreshToken { get; set; } = null!;
        public DateTime accessTokenExpiry { get; set; }
        public DateTime refreshTokenExpiry { get; set; }
    }
}
