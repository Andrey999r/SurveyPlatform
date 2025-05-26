using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smth.Data;
using Smth.Interfaces;
using Smth.Models;
using Smth.ViewModel;

namespace Smth.Controllers
{
    [Authorize]
    public class SurveysController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        private readonly IEmailService _emailSvc;

        public SurveysController(ApplicationDbContext ctx, IEmailService emailService)
        {
            _ctx = ctx;
            _emailSvc = emailService;
        }

        /* ───────── Вспомогательные ───────── */

        private int CurrentUserId => int.TryParse(User.FindFirstValue("UserId"), out var id) ? id : 0;
        private bool IsAdmin() => User.IsInRole("Admin");

        /* ─────────────────────────────────── */

        #region Создание опроса

        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(string name, string description, string[] questions)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");

            var survey = new Survey
            {
                Name = name,
                Description = description,
                ApplicationUserId = CurrentUserId,
                Questions = questions
                            .Where(q => !string.IsNullOrWhiteSpace(q))
                            .Select(q => new Question { Text = q })
                            .ToList()
            };

            _ctx.Surveys.Add(survey);
            _ctx.SaveChanges();
            return RedirectToAction(nameof(Created));
        }

        #endregion

        #region Списки опросов

        public IActionResult Created()
        {
            var list = IsAdmin()
                ? _ctx.Surveys.ToList()
                : _ctx.Surveys.Where(s => s.ApplicationUserId == CurrentUserId).ToList();

            return View(list);
        }

        public IActionResult Completed()
        {
            var me = _ctx.Users.Find(CurrentUserId);
            if (me is null) return BadRequest("Пользователь не найден");

            var surveys = _ctx.Surveys
                              .Include(s => s.Participants)
                              .ThenInclude(p => p.Answers)
                              .ThenInclude(a => a.Question)
                              .Where(s => s.Participants.Any(p => p.Email == me.Email))
                              .ToList();

            ViewBag.UserEmail = me.Email;
            return View(surveys);
        }

        #endregion

        #region Детали / удаление опроса

        public IActionResult Details(int id)
        {
            var survey = _ctx.Surveys
                             .Include(s => s.Participants)
                             .ThenInclude(p => p.Answers)
                             .ThenInclude(a => a.Question)
                             .FirstOrDefault(s => s.Id == id);

            if (survey is null) return NotFound();
            if (!IsAdmin() && survey.ApplicationUserId != CurrentUserId) return Unauthorized();

            var vm = new SurveyDetailsViewModel
            {
                Id = survey.Id,
                Name = survey.Name,
                Description = survey.Description,
                Participants = survey.Participants.Select(p => new ParticipantViewModel
                {
                    Id = p.Id,
                    ParticipantName = p.ParticipantName,
                    Email = p.Email,
                    CompletedAt = p.CompletedAt,
                    Answers = p.Answers.Select(a => new AnswerViewModel
                    {
                        Id = a.Id,
                        Text = a.Question.Text,
                        ResponseText = a.ResponseText
                    }).ToList()
                }).ToList()
            };
            return View(vm);
        }

        public IActionResult Delete(int id)
        {
            var survey = _ctx.Surveys
                             .Include(s => s.Participants).ThenInclude(p => p.Answers)
                             .FirstOrDefault(s => s.Id == id);

            if (survey is null) return NotFound();
            if (!IsAdmin() && survey.ApplicationUserId != CurrentUserId) return Unauthorized();

            foreach (var p in survey.Participants) _ctx.Answers.RemoveRange(p.Answers);
            _ctx.Participants.RemoveRange(survey.Participants);
            _ctx.Surveys.Remove(survey);
            _ctx.SaveChanges();

            return RedirectToAction(nameof(Created));
        }

        #endregion

        #region Редактирование контента опроса

        public IActionResult EditContent(int id)
        {
            var survey = _ctx.Surveys.Include(s => s.Questions).FirstOrDefault(s => s.Id == id);
            if (survey is null) return NotFound();
            if (!IsAdmin() && survey.ApplicationUserId != CurrentUserId) return Unauthorized();

            var vm = new EditSurveyViewModel
            {
                Id = survey.Id,
                Name = survey.Name,
                Description = survey.Description,
                Questions = survey.Questions
                                   .Select(q => new QuestionEditModel { Id = q.Id, Text = q.Text })
                                   .ToList()
            };
            return View("EditContent", vm);
        }

        [HttpPost]
        public IActionResult EditContent(EditSurveyViewModel model)
        {
            var survey = _ctx.Surveys.Include(s => s.Questions).FirstOrDefault(s => s.Id == model.Id);
            if (survey is null) return NotFound();
            if (!IsAdmin() && survey.ApplicationUserId != CurrentUserId) return Unauthorized();

            survey.Name = model.Name?.Trim();
            survey.Description = model.Description?.Trim();

            foreach (var qVm in model.Questions)
            {
                var q = survey.Questions.FirstOrDefault(x => x.Id == qVm.Id);
                if (q != null) q.Text = qVm.Text?.Trim();
            }
            _ctx.SaveChanges();

            TempData["Success"] = "Опрос обновлён";
            return RedirectToAction(nameof(Details), new { id = survey.Id });
        }

        #endregion

        #region Шаринг

        public IActionResult Share(int id)
        {
            var survey = _ctx.Surveys.Find(id);
            if (survey is null) return NotFound();
            if (!IsAdmin() && survey.ApplicationUserId != CurrentUserId) return Unauthorized();

            ViewBag.ShareLink = Url.Action(nameof(TakeSurvey), "Surveys", new { id }, Request.Scheme);
            ViewBag.SurveyId = id;
            return View();
        }

        [HttpPost]
        public IActionResult SendSurveyInvitation(int surveyId, string recipientEmail)
        {
            var survey = _ctx.Surveys.Find(surveyId);
            if (survey is null) return NotFound("Опрос не найден");
            if (!IsAdmin() && survey.ApplicationUserId != CurrentUserId) return Unauthorized();

            try
            {
                var link = Url.Action(nameof(TakeSurvey), "Surveys", new { id = surveyId }, Request.Scheme);
                _emailSvc.SendSurveyInvitation(recipientEmail, link);
                ViewBag.IsSuccess = true;
                ViewBag.Message = "Приглашение отправлено!";
            }
            catch (Exception ex)
            {
                ViewBag.IsSuccess = false;
                ViewBag.Message = ex.Message;
            }
            return View("ShareSurveyResult");
        }

        #endregion

        #region Прохождение опроса

        [HttpGet, AllowAnonymous]
        public IActionResult TakeSurvey(int id)
        {
            var s = _ctx.Surveys.Where(x => x.Id == id)
                                .Select(x => new
                                {
                                    x.Id,
                                    x.Name,
                                    x.Description,
                                    Questions = x.Questions.Select(q => new { q.Id, q.Text })
                                })
                                .FirstOrDefault();
            return s is null ? NotFound() : View(s);
        }

        [HttpPost, AllowAnonymous]
        public IActionResult TakeSurvey(int id, string participantName,
                                        string participantEmail, string[] answers)
        {
            var survey = _ctx.Surveys.Include(s => s.Questions).FirstOrDefault(s => s.Id == id);
            if (survey is null) return NotFound("Опрос не найден");
            if (answers is null || answers.Length == 0) return BadRequest("Ответы пусты");

            var myEmail = _ctx.Users.Find(CurrentUserId)?.Email ?? participantEmail;

            var participant = _ctx.Participants
                                  .FirstOrDefault(p => p.SurveyId == id && p.Email == myEmail);

            if (participant is null)
            {
                participant = new Participant
                {
                    ParticipantName = participantName ?? "Аноним",
                    Email = myEmail,
                    SurveyId = id,
                    CompletedAt = DateTime.UtcNow.AddHours(3)
                };
                _ctx.Participants.Add(participant);
                _ctx.SaveChanges();
            }

            for (int i = 0; i < survey.Questions.Count && i < answers.Length; i++)
                _ctx.Answers.Add(new Answer
                {
                    ParticipantId = participant.Id,
                    QuestionId = survey.Questions[i].Id,
                    ResponseText = answers[i]
                });

            _ctx.SaveChanges();

            return User.Identity.IsAuthenticated
                ? RedirectToAction(nameof(Completed))
                : RedirectToAction(nameof(ThankYou), new { participantName = participant.ParticipantName });
        }

        #endregion

        #region Управление участниками

        public IActionResult Info(int participantId)
        {
            var p = _ctx.Participants
                        .Include(x => x.Survey)
                        .Include(x => x.Answers).ThenInclude(a => a.Question)
                        .FirstOrDefault(x => x.Id == participantId);

            if (p == null) return NotFound();
            if (!IsAdmin() && p.Survey.ApplicationUserId != CurrentUserId)
                return Unauthorized();

            /* — кто может редактировать ответы — */
            bool canEdit = IsAdmin();          // ← только админ

            /* — куда вернёмся кнопкой «Назад» — */
            if (IsAdmin())
            {
                ViewBag.BackAction = "Users";  // список пользователей
                ViewBag.BackId = null;
            }
            else                               // создатель опроса
            {
                ViewBag.BackAction = "Details";
                ViewBag.BackId = p.SurveyId;
            }

            var vm = new ParticipantViewModel
            {
                Id = p.Id,
                ParticipantName = p.ParticipantName,
                Email = p.Email,
                SurveyName = p.Survey?.Name ?? "",
                CanEdit = canEdit,
                Answers = p.Answers.Select(a => new AnswerViewModel
                {
                    Id = a.Id,
                    Text = a.Question?.Text ?? "(удалён)",
                    ResponseText = a.ResponseText
                }).ToList()
            };
            return View(vm);
        }

        /* ───────────────────────────────────────────────────────────── */

        [HttpGet]
        public IActionResult InfoSurvey(int participantId)
        {
            var participant = _ctx.Participants
                                  .Include(p => p.Survey)
                                  .Include(p => p.Answers).ThenInclude(a => a.Question)
                                  .FirstOrDefault(p => p.Id == participantId);
            if (participant == null) return NotFound("Участник не найден.");

            var currentUser = _ctx.Users.Find(CurrentUserId);
            string currentEmail = currentUser?.Email ?? "";

            bool canSee = IsAdmin() ||
                          participant.Survey.ApplicationUserId == CurrentUserId ||
                          (!string.IsNullOrEmpty(currentEmail) &&
                           currentEmail.Equals(participant.Email, StringComparison.OrdinalIgnoreCase));

            if (!canSee) return Unauthorized();

            var vm = new ParticipantViewModel
            {
                Id = participant.Id,
                ParticipantName = participant.ParticipantName,
                Email = participant.Email,
                SurveyName = participant.Survey?.Name ?? "",
                CanEdit = false,    // только просмотр
                Answers = participant.Answers.Select(a => new AnswerViewModel
                {
                    Text = a.Question?.Text ?? "(удалён)",
                    ResponseText = a.ResponseText
                }).ToList()
            };

            /* — кнопка «Назад» — */
            if (IsAdmin())
            {
                ViewBag.BackAction = "Users";
                ViewBag.BackId = null;
            }
            else if (currentEmail.Equals(participant.Email, StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.BackAction = "Completed";   // сам участник
                ViewBag.BackId = null;
            }
            else                                    // автор опроса
            {
                ViewBag.BackAction = "Details";
                ViewBag.BackId = participant.SurveyId;
            }

            return View("InfoSurvey", vm);
        }


        [HttpPost]
        public IActionResult UpdateAnswers(ParticipantViewModel model)
        {
            var participant = _ctx.Participants
                                  .Include(p => p.Survey)
                                  .FirstOrDefault(p => p.Id == model.Id);

            if (participant is null) return NotFound();
            if (!IsAdmin())
                return Unauthorized();

            foreach (var ansVm in model.Answers)
            {
                var ans = _ctx.Answers.FirstOrDefault(a => a.Id == ansVm.Id &&
                                                           a.ParticipantId == participant.Id);
                if (ans != null)
                    ans.ResponseText = ansVm.ResponseText?.Trim();
            }

            _ctx.SaveChanges();
            TempData["Success"] = "Ответы обновлены";
            return RedirectToAction(nameof(Info), new { participantId = participant.Id });
        }

        [HttpPost]
        public IActionResult DeleteParticipant(int participantId)
        {
            var p = _ctx.Participants
                        .Include(x => x.Survey)
                        .Include(x => x.Answers)
                        .FirstOrDefault(x => x.Id == participantId);

            if (p is null) return NotFound();
            if (!IsAdmin() && p.Survey.ApplicationUserId != CurrentUserId) return Unauthorized();

            _ctx.Answers.RemoveRange(p.Answers);
            _ctx.Participants.Remove(p);
            _ctx.SaveChanges();

            TempData["Success"] = "Участник удалён";
            return RedirectToAction(nameof(Details), new { id = p.SurveyId });
        }

        #endregion

        #region Благодарность

        [AllowAnonymous]
        public IActionResult ThankYou(string participantName)
        {
            ViewBag.ParticipantName = participantName;
            return View();
        }

        #endregion

        #region Управление пользователями (Admin)

        [Authorize(Policy = "AdminOnly")]
        public IActionResult Users()
        {
            var users = _ctx.Users.OrderBy(u => u.Username).ToList();
            return View("Users", users);
        }

        [Authorize(Policy = "AdminOnly"), HttpPost]
        public IActionResult DeleteUser(int id)
        {
            if (id == CurrentUserId)
            {
                TempData["Error"] = "Нельзя удалить собственный аккаунт";
                return RedirectToAction(nameof(Users));
            }

            var user = _ctx.Users
                           .Include(u => u.Surveys)
                               .ThenInclude(s => s.Participants)
                                   .ThenInclude(p => p.Answers)
                           .FirstOrDefault(u => u.Id == id);

            if (user is null)
            {
                TempData["Error"] = "Пользователь не найден";
                return RedirectToAction(nameof(Users));
            }

            foreach (var s in user.Surveys)
            {
                foreach (var p in s.Participants)
                    _ctx.Answers.RemoveRange(p.Answers);

                _ctx.Participants.RemoveRange(s.Participants);
            }
            _ctx.Surveys.RemoveRange(user.Surveys);
            _ctx.Users.Remove(user);
            _ctx.SaveChanges();

            TempData["Success"] = "Пользователь удалён";
            return RedirectToAction(nameof(Users));
        }

        [Authorize(Policy = "AdminOnly")]
        public IActionResult UserSurveys(int id)
        {
            var user = _ctx.Users
                           .Include(u => u.Surveys)
                           .FirstOrDefault(u => u.Id == id);

            if (user is null) return NotFound("Пользователь не найден");

            ViewBag.UserName = user.Username;
            ViewBag.ActiveTab = "Users";
            return View("UserSurveys", user.Surveys);
        }

        #endregion
    }
}
