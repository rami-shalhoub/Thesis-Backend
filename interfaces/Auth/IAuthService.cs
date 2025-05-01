using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOs.auth;
using Backend.models;

namespace Backend.services.auth
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(RegisterDto registerDto);
        Task<User> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto);
        Task<User> DeleteUserAsync(Guid id);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
        Task LogoutAsync(string accessToken);
    }
}
