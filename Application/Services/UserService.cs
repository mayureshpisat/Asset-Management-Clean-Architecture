using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Domain.Entities;
using Application.DTO;


namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IPasswordHasher<User> _passwordHasher;
        public UserService(IPasswordHasher<User> passwordHasher) { 
            _passwordHasher = passwordHasher;

        }

        public async Task RegisterUser(RegisterDTO request)
        {
            try
            {

            }
            catch (Exception ex)
            {
            }

        }
    }
}
