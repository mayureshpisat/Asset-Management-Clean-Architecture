using Application.DTO;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces
{
    public interface IUserRepository
    {

        Task<User> GetUserByNameAsync(string username);
        Task<User> GetUserByEmailAsync(string email);

        Task<User> GetUserById(int userId);
        Task AddUserAsync(User user);

        Task SaveChangesAsync();

        Task<RefreshToken> GetRefreshToken(User user);

        Task SaveRefreshToken(RefreshToken refreshToken);


    }
}
