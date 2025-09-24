using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dotnet8_mvc_auth_boilerplate.Data;
using dotnet8_mvc_auth_boilerplate.Models;
using dotnet8_mvc_auth_boilerplate.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet8_mvc_auth_boilerplate.Controllers
{
    [Authorize]
    public class UserController : BaseController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: User
        public async Task<IActionResult> Index(string username, string loginProvider, int page = 1, int pageSize = 10)
        {
            ViewData["Module"] = "User";
            ViewData["CurrentFilter"] = username;
            ViewData["LoginProviderFilter"] = loginProvider;
            
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Prevent too large page sizes

            var users = await _userManager.Users.ToListAsync();
            var userModels = new List<UserModel>();

            foreach (var user in users)
            {
                var loginProviders = await _context.UserLogins
                    .Where(l => l.UserId == user.Id)
                    .Select(l => l.LoginProvider)
                    .ToListAsync();

                userModels.Add(new UserModel
                {
                    User = user,
                    LoginProviders = loginProviders
                });
            }

            // Apply filters
            if (!string.IsNullOrEmpty(username))
            {
                userModels = userModels.Where(u => u.User.UserName != null && u.User.UserName.Contains(username, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(loginProvider))
            {
                if (loginProvider.Equals("Local", StringComparison.OrdinalIgnoreCase))
                {
                    userModels = userModels.Where(u => u.LoginProviders == null || !u.LoginProviders.Any()).ToList();
                }
                else
                {
                    userModels = userModels.Where(u => u.LoginProviders != null && u.LoginProviders.Contains(loginProvider, StringComparer.OrdinalIgnoreCase)).ToList();
                }
            }

            // Create paginated list
            var paginatedUsers = PaginatedList<UserModel>.Create(userModels, page, pageSize);

            // Set up pagination info for the view component
            var routeValues = new Dictionary<string, string> { { "pageSize", pageSize.ToString() } };
            if (!string.IsNullOrEmpty(username))
            {
                routeValues.Add("username", username);
            }
            if (!string.IsNullOrEmpty(loginProvider))
            {
                routeValues.Add("loginProvider", loginProvider);
            }

            ViewBag.PaginationInfo = new PaginationInfo
            {
                CurrentPage = paginatedUsers.PageIndex,
                TotalPages = paginatedUsers.TotalPages,
                TotalItems = paginatedUsers.TotalCount,
                PageSize = paginatedUsers.PageSize,
                HasPrevious = paginatedUsers.HasPreviousPage,
                HasNext = paginatedUsers.HasNextPage,
                ActionName = "Index",
                ControllerName = "User",
                RouteValues = routeValues
            };

            return View(paginatedUsers);
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var logins = await _userManager.GetLoginsAsync(user);
            var loginProviders = await _context.UserLogins
                .Where(l => l.UserId == user.Id)
                .Select(l => l.LoginProvider)
                .ToListAsync();

            var viewModel = new UserModel
            {
                User = user,
                Claims = claims,
                Logins = logins,
                LoginProviders = loginProviders
            };

            ViewData["Module"] = "User";
            return View(viewModel);
        }

        // GET: User/Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var logins = await _userManager.GetLoginsAsync(user);
            var loginProviders = await _context.UserLogins
                .Where(l => l.UserId == user.Id)
                .Select(l => l.LoginProvider)
                .ToListAsync();
                
            var viewModel = new UserModel
            {
                User = user,
                Claims = claims,
                Logins = logins,
                LoginProviders = loginProviders
            };

            ViewData["Module"] = "User";
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Placeholder: wire to actual Edit view later
            TempData["StatusMessage"] = "Edit user screen not implemented yet.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Clear lockout
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
            await _userManager.ResetAccessFailedCountAsync(user);

            TempData["StatusMessage"] = "Account unlocked.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
