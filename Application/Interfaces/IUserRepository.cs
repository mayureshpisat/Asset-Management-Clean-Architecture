﻿using Application.DTO;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserRepository
    {

        Task<User> GetUserByNameAsync(string username);
        Task<User> GetUserByEmailAsync(string email);


    }
}
