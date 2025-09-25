using Application.DTO;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;


namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        public UserService(IPasswordHasher<User> passwordHasher, IUserRepository userRepository, IConfiguration configuration)
        {
            _passwordHasher = passwordHasher;
            _userRepository = userRepository;
            _configuration = configuration;
        }
        private string GenerateRefreshToken(int length = 32)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be a positive value.");
            }

            byte[] randomNumber = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }

        public async Task RegisterUser(RegisterDTO request)
        {
            try
            {
                var userByUsername = await _userRepository.GetUserByNameAsync(request.Username);
                if (userByUsername != null)
                {
                    throw new Exception("Username already exists");

                }
                var userByEmail = await _userRepository.GetUserByEmailAsync(request.Email);
                if (userByEmail != null)
                {
                    throw new Exception("Email already exists");

                }
                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    Role = "Viewer",
                    CreatedAtUtc = DateTime.UtcNow
                };

                user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

                await _userRepository.AddUserAsync(user);
                await _userRepository.SaveChangesAsync();



            }
            catch (DbUpdateException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
            

        }


        public async Task<List<string>> LoginUser(LoginDTO request)
        {
            try
            {
                var user = await _userRepository.GetUserByNameAsync(request.Username);
                if (user == null)
                {
                    throw new Exception("Invalid Username or Password");
                }
                var validatePassword = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
                if (validatePassword == PasswordVerificationResult.Failed)
                    throw new Exception("Invalid Username or Password");

                //refresh token for user
                var refreshToken = await _userRepository.GetRefreshToken(user);
                string token = "";
                if(refreshToken == null)
                {
                    token = GenerateRefreshToken();
                    refreshToken = new RefreshToken
                    {
                        User = user,
                        Token = token,
                        ExpiresAt = DateTime.UtcNow.AddDays(7),
                        IsRevoked = false
                    };
                }
                
                await _userRepository.SaveRefreshToken(refreshToken);
                await _userRepository.SaveChangesAsync();

                string tokenString = GenerateToken(user);
                return new List<string> { tokenString, token};
            }
            catch(DbUpdateException ex)
            {
                throw;
            }catch(Exception ex)
            {
                throw;
            }
            
        }

        public async Task<User> GetUser(int userId)
        {
            var user = await _userRepository.GetUserById(userId);
            return user;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            return user;
        }



        private string GenerateToken(User user)
        {
            // Create claims
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
    };

            //generate JWT 
            var jwtSettings = _configuration.GetSection("Jwt"); //plain string 
            //jwt sign in algos need a cryptographic key object 
            //wrap key into a security key object that jwt can understand
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); //header {type: jwt, algo: SHA256)

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"])),
                signingCredentials: creds
             );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token); //generates a token based on the creds and encrypts the payload

            //result = {Base64Url(Header)}.{Base64Url(Payload)}.{Base64Url(Signature)}
            //Signature is generated when WriteToken takes algo type and key from the creds and uses SHA256(header.payload,key);

            return tokenString;
        }
    }
}
