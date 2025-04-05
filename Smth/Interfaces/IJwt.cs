using Smth.Data;

namespace Smth.Interfaces
{
    /// <summary>
    /// Интерфейс для сервиса генерации JWT-токенов.
    /// </summary>
    public interface IJwt
    {
        /// <summary>
        /// Генерирует JWT-токен для указанного пользователя.
        /// </summary>
        /// <param name="user">Пользователь, для которого создаётся токен.</param>
        /// <returns>Строка с JWT-токеном.</returns>
        string GenerateToken(ApplicationUser user);
    }
}
