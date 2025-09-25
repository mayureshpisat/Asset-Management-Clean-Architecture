using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AssetDbContext _dbContext;
        public UserRepository(AssetDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> GetUserByNameAsync(string username)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<User> GetUserById(int userId)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }


        public async Task AddUserAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);

        }
        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public async Task<RefreshToken> GetRefreshToken(User user)
        {
            var refreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(r => r.UserId == user.Id);
            return refreshToken;
        }

        public async Task SaveRefreshToken(RefreshToken refreshToken)
        {
            await _dbContext.RefreshTokens.AddAsync(refreshToken);
        }


    }
}
