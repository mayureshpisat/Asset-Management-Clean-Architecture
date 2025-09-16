﻿using System.ComponentModel.DataAnnotations;

namespace Application.DTO
{
    public class RegisterDTO
    {
        [Required]
        [MinLength(2)]
        [MaxLength(30)]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$",
            ErrorMessage = "Username can only contain letters, numbers, hyphens (-), and underscores (_) (upto 30 characters)")]
        public string Username { get; set; }


        [Required]
        [EmailAddress (ErrorMessage ="Invalid email format")]
        public string Email { get; set; }

        [Required]
        [MinLength(5)]
        [MaxLength(30)]
        [RegularExpression(@".*",
            ErrorMessage = "Password should be of minimum 5 characters an maximum 30 characters")]
        public string Password { get; set; }
    }
}
