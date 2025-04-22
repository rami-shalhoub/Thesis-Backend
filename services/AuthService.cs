using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.DTOs.auth;
using Backend.models;
using Backend.repositories.auth;
using Backend.services.auth;
using Microsoft.AspNetCore.Identity;

namespace Backend.services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthService(IAuthRepository userRepository, IMapper mapper, IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
        }

        public async Task<User> RegisterAsync(RegisterDto registerDto)
        {
            if (await _userRepository.UserExistsAsync(registerDto.email))
                throw new ApplicationException("Email already exists");


            var user = _mapper.Map<User>(registerDto);
            user.password = _passwordHasher.HashPassword(user, registerDto.password);
            return await _userRepository.AddAsync(user);
        }

        public async Task<User> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto)
        {
            var user = await _userRepository.GetByIDAsync(id);

            if (user == null)
                throw new ApplicationException("User not found");

            _mapper.Map(updateUserDto, user);
            
            if (!string.IsNullOrEmpty(updateUserDto.password))
                user.password = _passwordHasher.HashPassword(user, updateUserDto.password);

            return await _userRepository.UpdateAsync(id, updateUserDto);
        }

        public Task LogoutAsync(string accessToken, string refreshToken)
        {
            throw new NotImplementedException();
        }
    }
}