using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using dotnet8_mvc_auth_boilerplate.Services;
using dotnet8_mvc_auth_boilerplate.Models;
using System;
using Microsoft.AspNetCore.Authentication;

namespace dotnet8_mvc_auth_boilerplate.Controllers
{
    public class AccountController : BaseController
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IADAuthenticationService _adAuthService;

        public AccountController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<AccountController> logger,
            IADAuthenticationService adAuthService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _adAuthService = adAuthService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            // If the user is already authenticated, send them to the dashboard instead of showing login page.
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl ?? "~/Dashboard"
            };
            
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // If user is already signed in (e.g., back-button or manual POST), just redirect.
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            // Ensure we have a valid returnUrl, defaulting to Dashboard
            if (string.IsNullOrEmpty(model.ReturnUrl))
            {
                model.ReturnUrl = "~/Dashboard";
            }

            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                if (!model.UseLocalLogin) // AD Authentication (default)
                {
                    var isValidCredentials = await _adAuthService.ValidateCredentialsAsync(model.Email, model.Password);
                    if (isValidCredentials)
                    {
                        var (user, isNewUser) = await _adAuthService.GetOrCreateUserAsync(model.Email);
                        if (user != null)
                        {
                            await _signInManager.SignInAsync(user, model.RememberMe);
                            _logger.LogInformation("User logged in with AD authentication.");
                            return RedirectToAction("Index", "Dashboard");
                        }
                    }
                    ModelState.AddModelError(string.Empty, "Invalid AD credentials.");
                    return View(model);
                }
                else // Local Authentication
                {
                    var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User logged in with local authentication.");
                        return RedirectToAction("Index", "Dashboard");
                    }
                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToAction(nameof(LoginWith2fa), new { returnUrl = model.ReturnUrl, rememberMe = model.RememberMe });
                    }
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("User account locked out.");
                        return RedirectToAction(nameof(Lockout));
                    }
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string? returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException("Unable to load two-factor authentication user.");
            }

            var model = new LoginWith2faViewModel
            {
                ReturnUrl = returnUrl ?? "~/Dashboard",
                RememberMe = rememberMe
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException("Unable to load two-factor authentication user.");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, model.RememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with 2FA.", user.Id);
                return LocalRedirect(model.ReturnUrl ?? Url.Content("~/"));
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            var model = new RegisterModel
            {
                ReturnUrl = returnUrl ?? "~/Dashboard"
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (string.IsNullOrEmpty(model.ReturnUrl))
            {
                model.ReturnUrl = "~/Dashboard";
            }

            if (ModelState.IsValid)
            {
                // Check if user with this User ID or Email already exists
                var existingUserByUserId = await _userManager.FindByNameAsync(model.UserId);
                var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);

                if (existingUserByUserId != null)
                {
                    ModelState.AddModelError("UserId", "A user with this Username already exists.");
                    return View(model);
                }

                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError("Email", "A user with this email already exists.");
                    return View(model);
                }

                // Create new user
                var user = new IdentityUser
                {
                    UserName = model.UserId, // Use Username as username
                    Email = model.Email,
                    EmailConfirmed = true // For local users, we'll set email as confirmed by default
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Local user created a new account with Username {UserId}.", model.UserId);

                    // Sign in the user after successful registration
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Dashboard");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
    }
}
