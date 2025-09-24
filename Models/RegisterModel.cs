using System.ComponentModel.DataAnnotations;

namespace dotnet8_mvc_auth_boilerplate.Models
{
    public class RegisterModel
    {
        [Required]
        [Display(Name = "Username")]
        // [RegularExpression(@"^[A-Z]\d{5}$", ErrorMessage = "Username must be in format like M00316 (one letter followed by 5 digits)")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string ReturnUrl { get; set; } = string.Empty;
    }
}