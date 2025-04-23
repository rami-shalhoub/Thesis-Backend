using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Backend.models;

namespace Backend.services.auth
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        Task<bool> IsRefreshTokenValid(Guid userId, string refreshToken);
        Task RevokeRefreshToken(string refreshToken, Guid userId);
    }
}
