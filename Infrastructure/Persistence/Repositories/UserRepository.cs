using Application.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AssetDbContext _dbContext;
        public UserRepository(AssetDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        Task<User> GetUserByNameAsync(string username)
        {
            return null;
        }
        Task<User> GetUserByEmailAsync(string email)
        {
            return null;
        }
    }
}
