namespace Smth.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// Представляет зарегистрированного пользователя системы.
    /// </summary>
    public class ApplicationUser
    {
        /// <summary>
        /// Уникальный идентификатор пользователя.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Имя пользователя (логин).
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Хэш пароля пользователя (не хранится в открытом виде).
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Email-адрес пользователя.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Список опросов, созданных пользователем.
        /// </summary>
        public List<Survey> Surveys { get; set; } = new List<Survey>();
        public string Role { get; set; } = "User";

    }
}
