using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOs.auth;
using Backend.models;

namespace Backend.repositories.auth
{
    public interface IAuthRepository
    {
        Task<List<User>> GetAllAsync();
        Task<User> GetByIDAsync(Guid id);
        Task<User> GetByEmailAsync(string email);
        Task<User> AddAsync(User user);
        Task<User> UpdateAsync(Guid id, UpdateUserDto user);
        Task<User> DeleteAsync(Guid id);
        Task<bool> UserExistsAsync(string email);
    }
}