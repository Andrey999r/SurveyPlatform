using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Smth.Data;
using Smth.Interfaces;

namespace Smth.Services;

public class JwtService : IJwt
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Конструктор, принимающий объект конфигурации для доступа к настройкам.
    /// </summary>
    /// <param name="configuration">Интерфейс конфигурации приложения</param>
    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Генерирует JWT токен для указанного пользователя.
    /// </summary>
    /// <param name="user">Пользователь, для которого генерируется токен</param>
    /// <returns>Строковое представление JWT токена</returns>
    public string GenerateToken(ApplicationUser user)
    {
        // Определяем набор утверждений (claims) для токена.
        // Здесь мы используем имя пользователя и уникальный идентификатор пользователя.
        // Smth.Services/JwtService.cs  (GenerateToken)
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Username),
        new Claim("UserId", user.Id.ToString()),
        new Claim(ClaimTypes.Role, user.Role),          // NEW
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };


        // Ключ для подписи токена. В реальном приложении его лучше хранить в безопасном месте (например, в конфигурации).
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("W9x4ABgt8Hh8htT5M2ecnb2E3Zn-il4fRNjHKFbziec"));
        // Создаем учетные данные на основе ключа и алгоритма HMAC-SHA256.
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Создаем сам JWT токен с заданными параметрами:
        // issuer - издатель токена,
        // audience - получатель токена,
        // claims - набор утверждений,
        // expires - время истечения токена,
        // signingCredentials - учетные данные для подписи токена.
        var token = new JwtSecurityToken(
            issuer: "Issuer",
            audience: "Audience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        // Преобразуем токен в строку и возвращаем его.
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
