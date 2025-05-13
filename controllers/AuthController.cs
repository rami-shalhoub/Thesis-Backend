using AutoMapper;
using Backend.DTOs.auth;
using Backend.models;
using Backend.services.auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Backend.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            try
            {
                var user = await _authService.RegisterAsync(registerDto);
                return Ok(new { user.userID, user.email, user.name, user.organisationID, user.role });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _authService.UpdateUserAsync(id, updateUserDto);
                return Ok(new { user.userID, user.email, user.name, user.organisationID });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var response = await _authService.LoginAsync(loginDto);
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var response = await _authService.RefreshTokenAsync(refreshTokenDto);
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Check if Authorization header exists
                if (!HttpContext.Request.Headers.ContainsKey("Authorization"))
                {
                    // No token provided, but we'll still return success for client-side logout
                    return Ok(new { message = "Logged out successfully (no token provided)" });
                }

                // Get the access token from the Authorization header
                var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    // Invalid token format, but we'll still return success for client-side logout
                    return Ok(new { message = "Logged out successfully (invalid token format)" });
                }

                var accessToken = authHeader.Replace("Bearer ", "");

                // Call the logout service
                await _authService.LogoutAsync(accessToken);
                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception but still return success for client-side logout
                Console.WriteLine($"Error during logout: {ex.Message}");
                return Ok(new { message = "Logged out successfully (server-side error handled)" });
            }
        }

        [Authorize]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var user = await _authService.DeleteUserAsync(id);
                return Ok(new { message = "User deleted successfully", user.userID, user.email, user.name });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the user", error = ex.Message });
            }
        }
    }
}
