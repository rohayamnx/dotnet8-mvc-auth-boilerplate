using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace dotnet8_mvc_auth_boilerplate.Models
{
    public class UserModel
    {
    // Initialized to satisfy non-nullable reference type requirements.
    public IdentityUser User { get; set; } = null!; // Will be set by controller before use
    public IList<Claim> Claims { get; set; } = new List<Claim>();
    public IList<UserLoginInfo> Logins { get; set; } = new List<UserLoginInfo>();
    public List<string> LoginProviders { get; set; } = new List<string>();
    }
}
