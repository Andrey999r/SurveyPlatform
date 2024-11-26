using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using SurveyPlatform.Models;

namespace SurveyPlatform.Services;
using System.Security.Cryptography;
using System.Text;
public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public User Authenticate(string username, string password)
    {
        string passwordHash = ComputeHash(password);
        var user = _context.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == passwordHash);
        return user;
    }

    public void SignIn(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("UserId", user.Id.ToString())
        };
        var claimsIdentity = new ClaimsIdentity(claims, "CookieAuthentication");
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true
        };
        _httpContextAccessor.HttpContext.SignInAsync("CookieAuthentication", new ClaimsPrincipal(claimsIdentity), authProperties);
    }

    public void SignOut()
    {
        _httpContextAccessor.HttpContext.SignOutAsync("CookieAuthentication");
    }

    private string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}