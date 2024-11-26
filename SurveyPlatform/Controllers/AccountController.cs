using SurveyPlatform.Models;
using SurveyPlatform.Services;

namespace SurveyPlatform.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly AuthService _authService;

    public AccountController(ApplicationDbContext context, AuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(string username, string password)
    {
        // Проверка на существование пользователя с таким именем
        if (_context.Users.Any(u => u.Username == username))
        {
            // Добавляем ошибку в ModelState
            ModelState.AddModelError("", "Пользователь с таким именем уже существует.");
            return View(); // Возвращаем обратно на страницу регистрации с ошибкой
        }

        // Создание нового пользователя
        var user = new User
        {
            Username = username,
            PasswordHash = ComputeHash(password)
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        // Авторизуем пользователя и перенаправляем на список опросов
        _authService.SignIn(user);
        return RedirectToAction("Index", "Survey");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        // Поиск пользователя по имени и паролю
        var user = _context.Users.SingleOrDefault(u => u.Username == username && u.PasswordHash == ComputeHash(password));

        if (user == null)
        {
            // Добавляем сообщение об ошибке, если логин или пароль неверны
            ModelState.AddModelError("", "Неверный логин или пароль.");
            return View();
        }

        // Авторизация пользователя
        _authService.SignIn(user);

        // Перенаправляем на список опросов
        return RedirectToAction("Index", "Survey");
    }



    public IActionResult Logout()
    {
        _authService.SignOut();
        return RedirectToAction("Index", "Home");
    }

    private string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}