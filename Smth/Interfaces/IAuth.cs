using Smth.Data;

namespace Smth.Interfaces
{
    /// <summary>
    /// Интерфейс для сервиса аутентификации и регистрации пользователей.
    /// </summary>
    public interface IAuth
    {
        /// <summary>
        /// Регистрирует нового пользователя.
        /// </summary>
        /// <param name="username">Имя пользователя (логин).</param>
        /// <param name="email">Email пользователя.</param>
        /// <param name="password">Пароль пользователя.</param>
        /// <returns>Созданный объект пользователя.</returns>
        ApplicationUser Register(string username, string email, string password);

        /// <summary>
        /// Аутентифицирует пользователя по имени пользователя (или email) и паролю.
        /// </summary>
        /// <param name="username">Имя пользователя или email.</param>
        /// <param name="password">Пароль.</param>
        /// <returns>Объект пользователя, если вход успешен; иначе — null.</returns>
        ApplicationUser Login(string username, string password);
    }
}
