using System;
using System.Linq;
using Smth.Data;
using Smth.Interfaces;

/// <summary>
/// Сервис аутентификации и регистрации пользователей.
/// Реализует интерфейс IAuth для управления процессами регистрации и входа пользователей.
/// </summary>
namespace Smth.Services
{
    public class AuthService : IAuth
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Конструктор, получающий экземпляр контекста базы данных для работы с пользователями.
        /// </summary>
        /// <param name="context">Контекст базы данных приложения</param>
        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Регистрирует нового пользователя, проводя валидацию входных данных.
        /// </summary>
        /// <param name="username">Имя пользователя (логин)</param>
        /// <param name="email">Email пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <returns>Созданный объект ApplicationUser</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если обязательные поля не заполнены</exception>
        /// <exception cref="InvalidOperationException">Выбрасывается, если логин или email уже используются</exception>
        public ApplicationUser Register(string username, string email, string password)
        {
            // Проверка обязательных полей: имя пользователя, email и пароль должны быть заполнены
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                throw new ArgumentException("Все поля обязательны для заполнения.");

            // Проверка уникальности логина
            if (_context.Users.Any(u => u.Username == username))
                throw new InvalidOperationException("Логин уже занят.");

            // Проверка уникальности email
            if (_context.Users.Any(u => u.Email == email))
                throw new InvalidOperationException("Email уже зарегистрирован.");

            // Создание нового объекта пользователя с хэшированием пароля для безопасности
            var user = new ApplicationUser
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };

            // Добавление нового пользователя в контекст и сохранение изменений в базе данных
            _context.Users.Add(user);
            _context.SaveChanges();

            // Возвращаем зарегистрированного пользователя
            return user;
        }

        /// <summary>
        /// Производит аутентификацию пользователя по имени (или email) и паролю.
        /// </summary>
        /// <param name="username">Имя пользователя или email для входа</param>
        /// <param name="password">Пароль пользователя</param>
        /// <returns>Объект ApplicationUser, если аутентификация прошла успешно; иначе null</returns>
        public ApplicationUser Login(string username, string password)
        {
            // Поиск пользователя в базе данных по имени пользователя или email
            var user = _context.Users.FirstOrDefault(u =>
                u.Username == username || u.Email == username);

            // Если пользователь не найден, можно вернуть null (или выбросить исключение по необходимости)
            if (user == null)
                return null;

            // Верификация пароля с использованием BCrypt
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                Console.WriteLine("Пароль не прошел верификацию");
                return null;
            }

            // Если верификация прошла успешно, возвращаем пользователя
            return user;
        }
    }
}
