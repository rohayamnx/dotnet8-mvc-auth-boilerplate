using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using dotnet8_mvc_auth_boilerplate.Models;

namespace dotnet8_mvc_auth_boilerplate.Services
{
    public interface IADAuthenticationService
    {
        Task<bool> ValidateCredentialsAsync(string username, string password);
        Task<(IdentityUser user, bool isNewUser)> GetOrCreateUserAsync(string username);
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class ADAuthenticationService : IADAuthenticationService
    {
        private readonly ADSettings _adSettings;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ADAuthenticationService> _logger;

        public ADAuthenticationService(
            IConfiguration configuration, 
            UserManager<IdentityUser> userManager,
            ILogger<ADAuthenticationService> logger)
        {
            _adSettings = configuration.GetSection("ActiveDirectory").Get<ADSettings>() ?? 
                throw new ArgumentException("ActiveDirectory settings are not configured", nameof(configuration));
            _userManager = userManager ?? 
                throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? 
                throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            try
            {
                _logger.LogInformation("Attempting to validate AD credentials for user: {Username}", username);
                var sw = System.Diagnostics.Stopwatch.StartNew();

                bool isValid = await Task.Run(() =>
                {
                    using (PrincipalContext context = new PrincipalContext(ContextType.Domain, _adSettings.Domain))
                    {
                        return context.ValidateCredentials(username, password);
                    }
                });

                sw.Stop();
                    
                _logger.LogInformation(
                    "AD validation completed in {ElapsedMs}ms for user {Username}. Result: {Result}", 
                    sw.ElapsedMilliseconds, username, isValid);
                    
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating AD credentials for user: {Username}", username);
                return false;
            }
        }

        public async Task<(IdentityUser user, bool isNewUser)> GetOrCreateUserAsync(string username)
        {
            // First try to find user by their AD login
            var userLogin = await _userManager.Users
                .Where(u => u.NormalizedUserName == _userManager.NormalizeName(username))
                .FirstOrDefaultAsync();

            if (userLogin != null)
            {
                return (userLogin, false);
            }

            // If not found by login, try by username
            var user = await _userManager.FindByNameAsync(username);
            bool isNewUser = false;

            if (user == null)
            {
                using (PrincipalContext context = new PrincipalContext(ContextType.Domain, _adSettings.Domain))
                {
                    UserPrincipal adUser = UserPrincipal.FindByIdentity(context, username);
                    if (adUser != null)
                    {
                        user = new IdentityUser
                        {
                            UserName = username,
                            Email = adUser.EmailAddress,
                            EmailConfirmed = true // AD users are considered pre-confirmed
                        };

                        var result = await _userManager.CreateAsync(user);
                        if (!result.Succeeded)
                        {
                            throw new Exception($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }

                        // Add AD login information
                        var loginInfo = new UserLoginInfo("ActiveDirectory", username, "Active Directory");
                        result = await _userManager.AddLoginAsync(user, loginInfo);
                        if (!result.Succeeded)
                        {
                            throw new Exception($"Failed to add AD login: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }

                        // Add claims from AD
                        var claims = new List<Claim>
                        {
                            new Claim("DisplayName", adUser.DisplayName ?? username),
                        };

                        // Get extended properties using DirectoryEntry
                        var directoryEntry = adUser.GetUnderlyingObject() as DirectoryEntry;
                        if (directoryEntry != null)
                        {
                            try
                            {
                                var department = directoryEntry.Properties["department"].Value?.ToString();
                                if (!string.IsNullOrEmpty(department))
                                {
                                    claims.Add(new Claim("Department", department));
                                    _logger.LogInformation("Added department claim for user {Username}: {Department}", username, department);
                                }
                                else
                                {
                                    _logger.LogWarning("No department found for user {Username}", username);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error getting department for user {Username}", username);
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(adUser.Description))
                            claims.Add(new Claim("JobTitle", adUser.Description));

                        foreach (var claim in claims)
                        {
                            result = await _userManager.AddClaimAsync(user, claim);
                            if (!result.Succeeded)
                            {
                                throw new Exception($"Failed to add claim: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                            }
                        }

                        isNewUser = true;
                    }
                }
            }

            if (user == null)
            {
                throw new InvalidOperationException($"Failed to create or retrieve user: {username}");
            }
            return (user, isNewUser);
        }
    }
}
