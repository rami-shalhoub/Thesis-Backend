using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Backend.data;
using Backend.models;
using Backend.services.auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Backend.services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ThesisDappDBContext _context;

        public JwtService(IConfiguration configuration, ThesisDappDBContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public string GenerateAccessToken(User user)
        {
            var secret = _configuration["JWT:Secret"] ?? throw new ArgumentNullException("JWT:Secret configuration is missing.");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.userID.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.name),
                new Claim(ClaimTypes.Role, user.role),
                new Claim("OrganisationID", user.organisationID)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JWT:AccessTokenExpirationMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            try
            {
                // Check for null or empty token
                if (string.IsNullOrEmpty(token))
                {
                    throw new ArgumentNullException(nameof(token), "Token cannot be null or empty");
                }

                // Set up token validation parameters with more lenient validation
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,  // More lenient validation for logout
                    ValidateIssuer = false,    // More lenient validation for logout
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? throw new ArgumentNullException("JWT:Secret configuration is missing."))),
                    ValidateLifetime = false,  // Don't validate lifetime for logout
                    RequireExpirationTime = false,
                    RequireSignedTokens = true
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                
                // Check if token is in valid JWT format
                if (!tokenHandler.CanReadToken(token))
                {
                    throw new SecurityTokenException("Token is not in a valid JWT format");
                }

                // Validate the token
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                // Additional validation
                if (securityToken is not JwtSecurityToken jwtSecurityToken)
                {
                    throw new SecurityTokenException("Invalid token type");
                }

                return principal;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating token: {ex.Message}");
                throw; // Rethrow to be handled by the caller
            }
        }

        public async Task<bool> IsRefreshTokenValid(Guid userId, string refreshToken)
        {
            var user = await _context.User.FindAsync(userId);
            if (user == null || user.refreshToken != refreshToken || user.tokenExpiry < DateTime.Now)
            {
                return false;
            }

            // Check if token is revoked
            var isRevoked = await _context.RevokedToken.AnyAsync(t => t.userID == userId && t.tokenID == refreshToken);
            return !isRevoked;
        }

        public async Task RevokeRefreshToken(string refreshToken, Guid userId)
        {
            var revokedToken = new RevokedToken
            {
                tokenID = refreshToken,
                userID = userId,
                revokedAt = DateTime.Now,
                expiry = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JWT:RefreshTokenExpirationDays"]))
            };

            await _context.RevokedToken.AddAsync(revokedToken);
            await _context.SaveChangesAsync();
        }
    }
}
