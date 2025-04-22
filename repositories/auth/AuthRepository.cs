using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.data;
using Backend.DTOs.auth;
using Backend.models;
using Microsoft.EntityFrameworkCore;

namespace Backend.repositories.auth
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ThesisDappDBContext _context;

        public AuthRepository(ThesisDappDBContext context)
        {
            _context = context;
        }

        public async Task<User> AddAsync(User user)
        {
            await _context.User.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> DeleteAsync(Guid id)
        {
            var user = await GetByIDAsync(id);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }
            _context.User.Remove(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.User.ToListAsync();
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.User.FirstOrDefaultAsync(u => u.email == email) ?? throw new InvalidOperationException("User not found.");
        }

        public async Task<User> GetByIDAsync(Guid id)
        {
            return await _context.User.FindAsync(id) ?? throw new InvalidOperationException("User not found.");
        }

        public async Task<User> UpdateAsync(Guid id, UpdateUserDto user)
        {
            var existingUser = await GetByIDAsync(id);
            if (existingUser == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            existingUser.email = user.email;
            existingUser.name = user.name;
            existingUser.organisationID = user.organisationID;

            _context.User.Update(existingUser);
            await _context.SaveChangesAsync();
            return existingUser;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _context.User.AnyAsync(u => u.email == email);
        }
    }
}