using System.Text;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using SurveyPlatform.Models;
using SurveyPlatform.ViewModel;

namespace SurveyPlatform.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class SurveyController : Controller
{
    private readonly ApplicationDbContext _context;

    public SurveyController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Survey/
    public async Task<IActionResult> Index()
    {
        var surveys = await _context.Surveys.ToListAsync();
        return View(surveys);
    }
    
    [HttpGet]
    // GET: /Survey/Create
    [Authorize] // Требуется авторизация
    public IActionResult Create()
    {
        return View();
    }
    // POST: /Survey/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Survey model, List<string> Questions, List<string> CorrectAnswers)
    {
        if (Questions == null || Questions.Count == 0)
        {
            ModelState.AddModelError("", "Добавьте хотя бы один вопрос.");
            return View(model);
        }

        while (CorrectAnswers.Count < Questions.Count)
        {
            CorrectAnswers.Add("");
        }

        model.Questions = string.Join(";", Questions);
        model.CorrectAnswers = string.Join(";", CorrectAnswers);

        _context.Surveys.Add(model);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    // GET: /Survey/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var survey = await _context.Surveys.FindAsync(id);
        if (survey != null)
        {
            _context.Surveys.Remove(survey);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
    // POST: /Survey/Delete/5

  
    // GET: /Survey/Details/5
   
  

    public async Task<IActionResult> Details(int id)
    {
        var survey = await _context.Surveys
            .Include(s => s.SurveyResponses) // Обязательно включаем связанные ответы
            .FirstOrDefaultAsync(s => s.Id == id);

        if (survey == null)
        {
            return NotFound();
        }

        var viewModel = new SurveyDetailsViewModel
        {
            Survey = survey,
            Questions = survey.Questions?.Split(';') ?? Array.Empty<string>(),
            CorrectAnswers = survey.CorrectAnswers?.Split(';') ?? Array.Empty<string>(),
            Responses = survey.SurveyResponses.Select(r => new SurveyResponseViewModel
            {
                RespondentName = r.RespondentName,
                Answers = r.Answers?.Split(';') ?? Array.Empty<string>()
            }).ToList()
        };

        return View(viewModel);
    }







    public IActionResult Take(int id)
    {
        var survey = _context.Surveys.FirstOrDefault(s => s.Id == id);

        if (survey == null)
        {
            return NotFound();
        }

        // Разбиваем вопросы и ответы из модели Survey
        var questions = survey.Questions.Split(';');
        var correctAnswers = survey.CorrectAnswers?.Split(';') ?? new string[questions.Length];

        ViewBag.Questions = questions;
        ViewBag.CorrectAnswers = correctAnswers;
        ViewBag.Survey = survey;

        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [HttpPost]

    public IActionResult SubmitResponse(int surveyId, string respondentName, List<string> answers)
    {
        if (string.IsNullOrWhiteSpace(respondentName))
        {
            ModelState.AddModelError("", "Введите ваше имя.");
            return RedirectToAction("Take", new { id = surveyId });
        }

        // Сохраняем ответы
        var response = new SurveyResponse
        {
            SurveyId = surveyId,
            RespondentName = respondentName,
            Answers = string.Join(";", answers) // Преобразуем ответы в строку
        };

        _context.SurveyResponses.Add(response);
        _context.SaveChanges();

        // Показываем сообщение благодарности
        ViewBag.Message = "Спасибо за участие в опросе!";
        return View("ThankYou");
    }



    public IActionResult ThankYou()
    {
        return View();
    }

 


}