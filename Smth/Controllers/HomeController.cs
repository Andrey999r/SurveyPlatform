using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smth.Data;

namespace Smth.Controllers
{
    /// <summary>
    /// Контроллер для работы с главной страницей приложения.
    /// Отвечает за отображение списка опросов, созданных пользователем.
    /// </summary>
    public class HomeController : Controller
    {
        // Поле для работы с базой данных
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Конструктор контроллера, внедряет зависимость ApplicationDbContext.
        /// </summary>
        /// <param name="context">Контекст базы данных для доступа к данным приложения</param>
        public HomeController(ApplicationDbContext context)
        {
            // Инициализация контекста базы данных
            _context = context;
        }

        /// <summary>
        /// Метод для отображения главной страницы.
        /// Выполняет проверку аутентификации и возвращает список опросов пользователя.
        /// </summary>
        /// <returns>Представление с данными опросов или перенаправление на форму входа</returns>
        public IActionResult Index()
        {
            // Получаем идентификатор пользователя из клеймов аутентифицированного пользователя
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            
            // Если идентификатор не найден, пользователь не аутентифицирован, перенаправляем на страницу входа
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Преобразуем значение клейма в целочисленный тип
            var userId = int.Parse(userIdClaim.Value);

            // Из базы данных выбираем опросы, созданные данным пользователем
            var surveys = _context.Surveys.Where(s => s.ApplicationUserId == userId).ToList();

            // Возвращаем представление с найденными опросами
            return View(surveys);
        }
    }
}
