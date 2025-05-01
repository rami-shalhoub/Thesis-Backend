using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Backend.DTOs.auth;
using Backend.models;
using Backend.repositories.auth;
using Backend.services.auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Backend.services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthService(
            IAuthRepository authRepository,
            IMapper mapper,
            IPasswordHasher<User> passwordHasher,
            IJwtService jwtService,
            IConfiguration configuration)
        {
            _authRepository = authRepository;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        public async Task<User> DeleteUserAsync(Guid id)
        {
            try
            {
                var user = await _authRepository.GetByIDAsync(id);
                if (user == null)
                {
                    throw new ApplicationException("User not found");
                }
                
                return await _authRepository.DeleteAsync(id);
            }
            catch (InvalidOperationException )
            {
                // Rethrow the exception to be handled by the controller
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while deleting the user: {ex.Message}", ex);
            }
        }

        public async Task<User> RegisterAsync(RegisterDto registerDto)
        {
            if (await _authRepository.UserExistsAsync(registerDto.email))
                throw new ApplicationException("Email already exists");


            var user = _mapper.Map<User>(registerDto);
            user.password = _passwordHasher.HashPassword(user, registerDto.password);
            return await _authRepository.AddAsync(user);
        }

        public async Task<User> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto)
        {
            var user = await _authRepository.GetByIDAsync(id);

            if (user == null)
                throw new ApplicationException("User not found");

            _mapper.Map(updateUserDto, user);

            if (!string.IsNullOrEmpty(updateUserDto.password))
                user.password = _passwordHasher.HashPassword(user, updateUserDto.password);

            return await _authRepository.UpdateAsync(id, updateUserDto);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _authRepository.GetByEmailAsync(loginDto.email);

            if (user == null)
                throw new ApplicationException("Invalid email or password");

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.password, loginDto.password);

            if (verificationResult == PasswordVerificationResult.Failed)
                throw new ApplicationException("Invalid email or password");

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Update user with refresh token
            user.refreshToken = refreshToken;
            user.tokenExpiry = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JWT:RefreshTokenExpirationDays"]));
            user.lastLogin = DateTime.Now;

            await _authRepository.UpdateAsync(user.userID, new UpdateUserDto
            {
                email = user.email,
                name = user.name,
                organisationID = user.organisationID,
                password = string.Empty // Not updating password here
            });

            return new AuthResponseDto
            {
                userID = user.userID,
                name = user.name,
                email = user.email,
                role = user.role,
                organisationID = user.organisationID,
                accessToken = accessToken,
                refreshToken = refreshToken,
                accessTokenExpiry = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JWT:AccessTokenExpirationMinutes"])),
                refreshTokenExpiry = user.tokenExpiry
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(refreshTokenDto.refreshToken);
            var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrEmpty(userIdString))
                throw new ApplicationException("Invalid token: User ID not found");

            var userId = Guid.Parse(userIdString);

            var isValid = await _jwtService.IsRefreshTokenValid(userId, refreshTokenDto.refreshToken);
            if (!isValid)
                throw new ApplicationException("Invalid refresh token");

            var user = await _authRepository.GetByIDAsync(userId);

            // Generate new tokens
            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // Update user with new refresh token
            user.refreshToken = newRefreshToken;
            user.tokenExpiry = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JWT:RefreshTokenExpirationDays"]));

            await _authRepository.UpdateAsync(user.userID, new UpdateUserDto
            {
                email = user.email,
                name = user.name,
                organisationID = user.organisationID,
                password = string.Empty // Not updating password here
            });

            // Revoke old refresh token
            await _jwtService.RevokeRefreshToken(refreshTokenDto.refreshToken, userId);

            return new AuthResponseDto
            {
                userID = user.userID,
                name = user.name,
                email = user.email,
                role = user.role,
                organisationID = user.organisationID,
                accessToken = newAccessToken,
                refreshToken = newRefreshToken,
                accessTokenExpiry = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JWT:AccessTokenExpirationMinutes"])),
                refreshTokenExpiry = user.tokenExpiry
            };
        }

        public async Task LogoutAsync(string accessToken)
        {
            try
            {
                // Check if token is empty or null
                if (string.IsNullOrEmpty(accessToken))
                {
                    Console.WriteLine("Logout attempted with empty token");
                    return; // Nothing to do with an empty token
                }

                // Try to get the principal from the token
                ClaimsPrincipal principal;
                try
                {
                    principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Token validation failed during logout: {ex.Message}");
                    return; // Can't proceed with invalid token
                }

                // Try to get the user ID from the principal
                var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
                if (string.IsNullOrEmpty(userIdString))
                {
                    Console.WriteLine("User ID not found in token claims");
                    return; // Can't proceed without user ID
                }

                // Parse the user ID
                Guid userId;
                try
                {
                    userId = Guid.Parse(userIdString);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to parse user ID: {ex.Message}");
                    return; // Can't proceed with invalid user ID
                }

                // Get the user and their refresh token
                var user = await _authRepository.GetByIDAsync(userId);
                if (user == null)
                {
                    Console.WriteLine($"User not found for ID: {userId}");
                    return; // User doesn't exist
                }

                // Revoke the refresh token if it exists
                if (!string.IsNullOrEmpty(user.refreshToken))
                {
                    await _jwtService.RevokeRefreshToken(user.refreshToken, userId);
                }

                // Clear the user's refresh token and expiry
                user.refreshToken = string.Empty;
                user.tokenExpiry = DateTime.MinValue;

                // Update the user
                await _authRepository.UpdateAsync(user.userID, new UpdateUserDto
                {
                    email = user.email,
                    name = user.name,
                    organisationID = user.organisationID,
                    password = string.Empty // Not updating password here
                });

                Console.WriteLine($"User {userId} logged out successfully");
            }
            catch (Exception ex)
            {
                // Log the exception but don't rethrow
                Console.WriteLine($"Error during logout process: {ex.Message}");
            }
        }
    }
}
