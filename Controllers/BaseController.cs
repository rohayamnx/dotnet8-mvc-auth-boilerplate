using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace dotnet8_mvc_auth_boilerplate.Controllers
{
    [Authorize]
    public abstract class BaseController : Controller
    {
        protected bool IsActiveDirectoryUser => User.HasClaim(c => c.Type == "LoginType" && c.Value == "ActiveDirectory");

        protected IActionResult HandleUnauthorizedAccess()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account");
            }
            return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
        }
    }
}
