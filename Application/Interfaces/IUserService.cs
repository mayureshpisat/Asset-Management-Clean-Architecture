using Application.DTO;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task RegisterUser(RegisterDTO request);

        Task<string> LoginUser(LoginDTO request);

        Task<User> GetUser(int userId);

        Task<User> GetUserByEmail(string email);
    }
}
