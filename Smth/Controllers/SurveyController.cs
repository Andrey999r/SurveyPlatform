using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smth.Data;
using Smth.Interfaces;
using Smth.Models;
using Smth.Services;
using Smth.ViewModel;

namespace Smth.Controllers
{
    /// <summary>
    /// Контроллер для управления опросами: создание, просмотр, редактирование, удаление, а также проведение опроса участниками.
    /// </summary>
    [Authorize]
    public class SurveysController : Controller
    {
        // Контекст базы данных для доступа к данным опросов, участников, вопросов и ответов.
        private readonly ApplicationDbContext _context;
        // Конфигурация приложения для получения параметров.
        private readonly IConfiguration _configuration;
        // Сервис для отправки email уведомлений об опросах.
        private readonly IEmailService _emailService;

        /// <summary>
        /// Конструктор контроллера, внедряющий зависимости: контекст базы данных, конфигурацию и сервис для email.
        /// </summary>
        /// <param name="context">Контекст базы данных приложения.</param>
        /// <param name="configuration">Конфигурация приложения.</param>
        /// <param name="emailService">Сервис для отправки email уведомлений.</param>
        public SurveysController(ApplicationDbContext? context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        /// <summary>
        /// Отображение формы создания нового опроса.
        /// </summary>
        /// <returns>Представление с формой создания опроса.</returns>
        public IActionResult Create()
        {
            // Возвращаем представление для создания опроса.
            return View();
        }

        /// <summary>
        /// Обработка данных формы создания опроса.
        /// Создаёт новый опрос, добавляет вопросы и сохраняет данные в базе.
        /// </summary>
        /// <param name="name">Название опроса.</param>
        /// <param name="description">Описание опроса.</param>
        /// <param name="questions">Массив вопросов, введённых пользователем.</param>
        /// <returns>Перенаправление на страницу со списком созданных опросов.</returns>
        [HttpPost]
        public IActionResult Create(string name, string description, string[] questions)
        {
            // Получаем идентификатор пользователя из клеймов
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Преобразуем идентификатор в число
            int parsedUserId = int.Parse(userId);

            // Создаём новый объект опроса и заполняем основными данными
            var survey = new Survey
            {
                Name = name,
                Description = description,
                ApplicationUserId = parsedUserId
            };

            // Для каждого непустого вопроса создаём объект Question и добавляем в список вопросов опроса
            foreach (var q in questions.Where(x => !string.IsNullOrEmpty(x)))
            {
                survey.Questions.Add(new Question { Text = q });
            }

            // Добавляем опрос в контекст и сохраняем изменения в базе данных
            _context.Surveys.Add(survey);
            _context.SaveChanges();

            // Перенаправляем пользователя на страницу "Created", где отображаются созданные опросы
            return RedirectToAction("Created", "Surveys");
        }

        /// <summary>
        /// Отображение списка опросов, созданных текущим пользователем.
        /// </summary>
        /// <returns>Представление со списком опросов.</returns>
        public IActionResult Created()
        {
            // Получаем идентификатор пользователя из клеймов
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            // Проверяем корректность идентификатора
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdd))
            {
                return BadRequest("Некорректный ID пользователя");
            }

            // Выбираем опросы, созданные данным пользователем
            var surveys = _context.Surveys.Where(s => s.ApplicationUserId == userIdd).ToList();
            return View(surveys);
        }

        /// <summary>
        /// Отображение списка опросов, в которых участвовал пользователь (прохождение опросов).
        /// </summary>
        /// <returns>Представление с информацией о пройденных опросах.</returns>
        public IActionResult Completed()
        {
            // Получаем идентификатор пользователя из клеймов
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            int.TryParse(userId, out int parsedUserId);

            // Извлекаем email пользователя по его ID
            var userEmail = _context.Users
                .Where(u => u.Id == parsedUserId)
                .Select(u => u.Email)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(userEmail))
            {
                return BadRequest("Не удалось получить email пользователя.");
            }

            // Загружаем опросы с участниками и их ответами, фильтруя по email участника
            var completedSurveys = _context.Surveys
                .Include(s => s.Owner) // Загружаем данные владельца опроса
                .Include(s => s.Participants) // Загружаем участников опроса
                .ThenInclude(p => p.Answers) // Загружаем ответы участников
                .ThenInclude(a => a.Question) // Загружаем вопросы, к которым даны ответы
                .Where(s => s.Participants.Any(p => p.Email == userEmail)) // Фильтруем опросы, где есть участник с данным email
                .ToList();

            // Сохраняем email пользователя во ViewBag для использования в представлении
            ViewBag.UserEmail = userEmail;

            return View(completedSurveys);
        }

        /// <summary>
        /// Отображение подробной информации об опросе, созданном пользователем.
        /// </summary>
        /// <param name="id">Идентификатор опроса.</param>
        /// <returns>Представление с подробностями опроса или ошибка NotFound.</returns>
        public IActionResult Details(int id)
        {
            // Получаем идентификатор пользователя для проверки прав доступа
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (userId == null)
                return RedirectToAction("Login", "Account");

            int parsedUserId = int.Parse(userId);

            // Извлекаем опрос, принадлежащий текущему пользователю, и преобразуем его в модель для представления
            var survey = _context.Surveys
                .Where(s => s.ApplicationUserId == parsedUserId && s.Id == id)
                .Select(s => new SurveyDetailsViewModel
                {
                    Name = s.Name,
                    Description = s.Description,
                    Participants = s.Participants.Select(p => new ParticipantViewModel
                    {
                        Id = p.Id, // Идентификатор участника
                        ParticipantName = p.ParticipantName,
                        Email = p.Email, // Email участника
                        CompletedAt = p.CompletedAt,
                        Answers = p.Answers.Select(a => new AnswerViewModel
                        {
                            Text = a.Question.Text,         // Текст вопроса
                            ResponseText = a.ResponseText     // Ответ участника
                        }).ToList()
                    }).ToList()
                })
                .FirstOrDefault();

            // Если опрос не найден, возвращаем NotFound
            if (survey == null)
                return NotFound();

            return View(survey);
        }

        /// <summary>
        /// Удаление опроса, принадлежащего текущему пользователю.
        /// Также удаляются все связанные данные участников и их ответы.
        /// </summary>
        /// <param name="id">Идентификатор опроса для удаления.</param>
        /// <returns>Перенаправление на страницу списка созданных опросов.</returns>
        public IActionResult Delete(int id)
        {
            // Получаем идентификатор пользователя из клеймов
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (userId == null)
                return RedirectToAction("Login", "Account");

            int parsedUserId = int.Parse(userId);

            // Находим опрос, принадлежащий текущему пользователю, с включением связанных участников и их ответов
            var survey = _context.Surveys
                .Include(s => s.Participants)
                .ThenInclude(p => p.Answers)
                .FirstOrDefault(s => s.Id == id && s.ApplicationUserId == parsedUserId);

            if (survey != null)
            {
                // Удаляем ответы каждого участника опроса
                foreach (var participant in survey.Participants)
                {
                    _context.Answers.RemoveRange(participant.Answers);
                }
                // Удаляем участников опроса
                _context.Participants.RemoveRange(survey.Participants);
                // Удаляем сам опрос
                _context.Surveys.Remove(survey);
                _context.SaveChanges();
            }

            return RedirectToAction("Created", "Surveys");
        }

        /// <summary>
        /// Отображение страницы для генерации ссылки для совместного прохождения опроса.
        /// </summary>
        /// <param name="id">Идентификатор опроса.</param>
        /// <returns>Представление с информацией для шаринга опроса или ошибка NotFound.</returns>
        public IActionResult Share(int id)
        {
            // Находим опрос по идентификатору
            var survey = _context.Surveys.FirstOrDefault(s => s.Id == id);
            if (survey == null)
                return NotFound();

            // Передаём идентификатор опроса и сгенерированную ссылку для прохождения опроса во ViewBag
            ViewBag.SurveyId = id;
            ViewBag.ShareLink = Url.Action("TakeSurvey", "Surveys", new { id }, Request.Scheme);

            return View();
        }

        /// <summary>
        /// Отправка приглашения на прохождение опроса по email.
        /// </summary>
        /// <param name="surveyId">Идентификатор опроса.</param>
        /// <param name="recipientEmail">Email получателя приглашения.</param>
        /// <returns>Представление результата отправки приглашения.</returns>
        [HttpPost]
        public IActionResult SendSurveyInvitation(int surveyId, string recipientEmail)
        {
            try
            {
                // Находим опрос по идентификатору
                var survey = _context.Surveys.FirstOrDefault(s => s.Id == surveyId);
                if (survey == null)
                {
                    ViewBag.Message = "Опрос не найден.";
                    ViewBag.IsSuccess = false;
                    return View("ShareSurveyResult");
                }

                // Генерируем ссылку для прохождения опроса
                var surveyLink = Url.Action("TakeSurvey", "Surveys", new { id = surveyId }, Request.Scheme);
                // Отправляем приглашение с помощью сервиса email
                _emailService.SendSurveyInvitation(recipientEmail, surveyLink);

                ViewBag.Message = "Приглашение успешно отправлено!";
                ViewBag.IsSuccess = true;
            }
            catch (Exception ex)
            {
                // В случае ошибки отправки, формируем сообщение об ошибке
                ViewBag.Message = "Ошибка при отправке: " + ex.Message;
                ViewBag.IsSuccess = false;
            }

            return View("ShareSurveyResult");
        }

        /// <summary>
        /// Отображение страницы прохождения опроса для неавторизованных пользователей.
        /// </summary>
        /// <param name="id">Идентификатор опроса.</param>
        /// <returns>Представление с данными опроса для прохождения.</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult TakeSurvey(int id)
        {
            // Извлекаем опрос с его вопросами для отображения
            var survey = _context.Surveys
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    Questions = s.Questions.Select(q => new { q.Id, q.Text })
                })
                .FirstOrDefault();

            return View(survey);
        }

        /// <summary>
        /// Обработка ответов участника при прохождении опроса.
        /// Создаёт или обновляет данные участника, сохраняет ответы.
        /// </summary>
        /// <param name="id">Идентификатор опроса.</param>
        /// <param name="participantName">Имя участника.</param>
        /// <param name="participantEmail">Email участника.</param>
        /// <param name="answers">Массив ответов на вопросы опроса.</param>
        /// <returns>Перенаправление в зависимости от статуса аутентификации участника.</returns>
        [HttpPost]
        [AllowAnonymous]
        public IActionResult TakeSurvey(int id, string participantName, string participantEmail, string[] answers)
        {
            // Проверяем, существует ли опрос с указанным идентификатором
            var survey = _context.Surveys.FirstOrDefault(s => s.Id == id);
            if (survey == null)
            {
                return NotFound("Опрос не найден.");
            }

            // Если массив ответов пустой, возвращаем ошибку
            if (answers == null || answers.Length == 0)
            {
                return BadRequest("Вы не ответили ни на один вопрос.");
            }

            // Пытаемся получить идентификатор пользователя (если он авторизован)
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            int.TryParse(userId, out int currentUserId);

            // Получаем email пользователя из базы данных (если пользователь авторизован)
            var userEmail = _context.Users
                .Where(u => u.Id == currentUserId)
                .Select(u => u.Email)
                .FirstOrDefault();

            // Проверяем, существует ли уже запись участия в опросе по данному email
            var existingParticipant = _context.Participants
                .FirstOrDefault(p => p.SurveyId == id && (p.Email == userEmail || p.Email == participantEmail));

            if (existingParticipant == null)
            {
                // Если записи нет, создаём нового участника
                existingParticipant = new Participant
                {
                    ParticipantName = participantName ?? "Аноним",
                    Email = userEmail ?? participantEmail, // Приоритет отдан email авторизованного пользователя
                    SurveyId = id,
                    CompletedAt = DateTime.UtcNow.AddHours(3) // Фиксируем время прохождения опроса
                };

                _context.Participants.Add(existingParticipant);
                _context.SaveChanges();
            }

            // Извлекаем вопросы опроса
            var surveyQuestions = _context.Questions
                .Where(q => q.SurveyId == id)
                .ToList();

            // Сохраняем ответы участника: сопоставляем каждый вопрос с ответом
            for (int i = 0; i < surveyQuestions.Count && i < answers.Length; i++)
            {
                _context.Answers.Add(new Answer
                {
                    ParticipantId = existingParticipant.Id,
                    QuestionId = surveyQuestions[i].Id,
                    ResponseText = answers[i]
                });
            }

            _context.SaveChanges();

            // Перенаправляем участника: если он авторизован, переходим на страницу Completed, иначе на страницу благодарности
            return User.Identity.IsAuthenticated
                ? RedirectToAction("Completed", "Surveys")
                : RedirectToAction("ThankYou", "Surveys", new { participantName = existingParticipant.ParticipantName });
        }

        /// <summary>
        /// Обновление email участника опроса.
        /// </summary>
        /// <param name="model">Модель, содержащая идентификатор участника и новый email.</param>
        /// <returns>Перенаправление на страницу информации об участнике или ошибка NotFound.</returns>
        [HttpPost]
        public IActionResult UpdateEmail(ParticipantViewModel model)
        {
            // Ищем участника опроса по идентификатору
            var participant = _context.Participants.Find(model.Id);
            if (participant == null)
            {
                return NotFound();
            }

            // Обновляем email участника и сохраняем изменения
            participant.Email = model.Email;
            _context.SaveChanges();

            TempData["Message"] = "Email успешно обновлен!";
            return RedirectToAction("Info", new { participantId = model.Id });
        }

        /// <summary>
        /// Отображение информации об участнике опроса.
        /// </summary>
        /// <param name="participantId">Идентификатор участника.</param>
        /// <returns>Представление с данными участника и его ответами.</returns>
        [HttpGet]
        public IActionResult Info(int participantId)
        {
            // Загружаем участника, включая его ответы и связанные вопросы
            var participant = _context.Participants
                .Include(p => p.Answers)
                .ThenInclude(a => a.Question) // Гарантируем, что данные вопросов доступны
                .FirstOrDefault(p => p.Id == participantId);

            if (participant == null)
                return NotFound();

            // Формируем модель представления с информацией об участнике
            var viewModel = new ParticipantViewModel
            {
                Id = participant.Id,
                ParticipantName = participant.ParticipantName,
                Email = participant.Email,
                Answers = (participant.Answers ?? new List<Answer>()).Select(a => new AnswerViewModel
                {
                    Text = a.Question?.Text ?? "Нет данных", // Проверяем, что вопрос не равен null
                    ResponseText = a.ResponseText
                }).ToList()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Удаление участника опроса и всех его ответов.
        /// </summary>
        /// <param name="participantId">Идентификатор участника для удаления.</param>
        /// <returns>Перенаправление на страницу деталей опроса или ошибка NotFound.</returns>
        [HttpPost]
        public IActionResult DeleteParticipant(int participantId)
        {
            // Загружаем участника вместе с его ответами
            var participant = _context.Participants
                .Include(p => p.Answers) // Загружаем связанные ответы
                .FirstOrDefault(p => p.Id == participantId);

            if (participant == null)
                return NotFound();

            // Удаляем сначала связанные ответы
            _context.Answers.RemoveRange(participant.Answers);
            // Затем удаляем самого участника
            _context.Participants.Remove(participant);

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Участник успешно удалён.";
            return RedirectToAction("Details", new { id = participant.SurveyId });
        }

        /// <summary>
        /// Отображение подробной информации об опросе участника.
        /// </summary>
        /// <param name="participantId">Идентификатор участника.</param>
        /// <returns>Представление с информацией о прохождении опроса участником.</returns>
        public IActionResult InfoSurvey(int participantId)
        {
            // Загружаем данные участника, его опрос, а также ответы и связанные вопросы
            var participant = _context.Participants
                .Include(p => p.Survey) // Загружаем опрос, к которому относится участник
                .Include(p => p.Answers)
                .ThenInclude(a => a.Question) // Загружаем данные вопросов
                .FirstOrDefault(p => p.Id == participantId);

            if (participant == null)
                return NotFound("Участник не найден.");

            // Формируем модель представления с подробной информацией об участнике и опросе
            var viewModel = new ParticipantViewModel
            {
                Id = participant.Id,
                ParticipantName = participant.ParticipantName,
                Email = participant.Email,
                SurveyName = participant.Survey?.Name ?? "Неизвестный опрос",
                Answers = participant.Answers.Select(a => new AnswerViewModel
                {
                    Text = a.Question?.Text ?? "Нет данных",
                    ResponseText = a.ResponseText
                }).ToList()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Отображение страницы благодарности после прохождения опроса.
        /// Доступно без авторизации.
        /// </summary>
        /// <param name="participantName">Имя участника опроса.</param>
        /// <returns>Представление благодарности.</returns>
        [AllowAnonymous]
        public IActionResult ThankYou(string participantName)
        {
            // Передаём имя участника через ViewBag для отображения в представлении
            ViewBag.ParticipantName = participantName;
            return View();
        }
    }
}
