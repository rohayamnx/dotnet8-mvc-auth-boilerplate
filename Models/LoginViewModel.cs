using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;

namespace dotnet8_mvc_auth_boilerplate.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Username / Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        [Display(Name = "Use Local Login")]
        public bool UseLocalLogin { get; set; }

        public string ReturnUrl { get; set; } = string.Empty;

        public IList<AuthenticationScheme>? ExternalLogins { get; set; }

        public LoginInputModel? Input { get; set; }

        public class LoginInputModel
        {
            [Required]
            [Display(Name = "Username / Email")]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }

            [Display(Name = "Use Local Login")]
            public bool UseLocalLogin { get; set; }
        }
    }
}
