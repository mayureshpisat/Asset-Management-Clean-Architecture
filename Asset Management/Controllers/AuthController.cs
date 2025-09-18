using Infrastructure.Persistence;
using Application.DTO;
using Domain.Entities;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Interfaces;

namespace Asset_Management.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly AssetDbContext _dbContext;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        public AuthController(AssetDbContext dbContext, IPasswordHasher<User> passwordHasher, IConfiguration configuration, IUserService userService)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _userService = userService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            if (ModelState.ContainsKey("Email"))
            {
                foreach (var error in ModelState["Email"].Errors)
                {
                    return BadRequest(error.ErrorMessage);
                }
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Username can only contain letters, numbers, hyphens (-), and underscores (_) (upto 30 characters)");
            }
            try
            {
                await _userService.RegisterUser(dto);

            }
            catch (DbUpdateException)
            {
                return BadRequest("Database exception");
            }
            catch(Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }

            return Ok("User Registered");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            try
            {
                string tokenString = await _userService.LoginUser(dto);

                //Save the token to a Cookie for security
                Response.Cookies.Append("token", tokenString, new CookieOptions
                {
                    HttpOnly = true, //token can't be accessed with js
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/"

                });
            }
            catch (DbUpdateException ex)
            {
            }
            catch(Exception ex)
            {
                return Unauthorized("Invalid Username or Password");
            }


            return Ok("User Login Successful");


        }

        [HttpGet("GetUserInfo")]
        [Authorize]
        public async Task<IActionResult> GetUserInfo()
        {

            var userClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            if(userClaim == null)
                return Unauthorized("User ID claim not found in token.");

            int userId = int.Parse(userClaim);
            var user = await _userService.GetUser(userId);
            if (user == null)
                return NotFound("User not found");
            Console.WriteLine("FROM GET USER INFO");
            Console.WriteLine(user.Username);
            int id = user.Id;
            string Username = user.Username;
            string Role = user.Role;

            return Ok( new {
                id = id,
                username = Username,
                role = Role
            });


        }
        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("token", new CookieOptions
            {
                HttpOnly = true, //token can't be accessed with js
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });
            return Ok("Logged Out");
        }


        [HttpPost("ExternalLogin")]
        public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginDTO dto)
        {

            Console.WriteLine("Controller started");
            if (string.IsNullOrEmpty(dto.IdToken))
                return BadRequest("ID Token is required");

            GoogleJsonWebSignature.Payload payload;
            try
            {
                // Verify the Google ID token
                payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken);
                // payload contains email, name, sub (Google user ID)


            }
            catch
            {
                return Unauthorized("Invalid Google ID Token");
            }

            // Check if user already exists
            var user = await _userService.GetUserByEmail(payload.Email);


            if (user == null)
            {
                // Create new user
                user = new User
                {
                    Username = payload.Name ?? payload.Email.Split('@')[0],
                    Email = payload.Email,
                    Role = "Viewer",
                    PasswordHash = null,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
            }

            // Generate claims for your custom JWT
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
            };

            // Generate JWT
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"])),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            //Save the token to a Cookie for security
            Response.Cookies.Append("token", tokenString, new CookieOptions
            {
                HttpOnly = true, //token can't be accessed with js
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"

            });

            // Return the token + user info to frontend
            return Ok(new
            {
                Token = tokenString,
                ExpiresAt = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"])),
                User = new
                {
                    id = user.Id,
                    username = user.Username,
                    role = user.Role
                }
            });
        }




    }


}
