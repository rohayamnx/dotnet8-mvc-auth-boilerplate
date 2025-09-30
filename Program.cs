using dotnet8_mvc_auth_boilerplate.Data;
using dotnet8_mvc_auth_boilerplate.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = true;
    // You might want to adjust these settings for AD users
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure application cookie options
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "dotnet8_mvc_auth_boilerplate_auth"; // Unique cookie name for this app
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // adjust as needed
    options.Events.OnRedirectToLogin = context =>
    {
        // Ensure we redirect to login page
        context.Response.Redirect("/Account/Login");
        return Task.CompletedTask;
    };
});

// Add AD authentication
builder.Services.AddAuthentication()
    .AddNegotiate();

// Register AD authentication service
if (OperatingSystem.IsWindows())
{
    builder.Services.AddScoped<IADAuthenticationService, ADAuthenticationService>();
}
else
{
    throw new PlatformNotSupportedException("Active Directory authentication is only supported on Windows.");
}
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
