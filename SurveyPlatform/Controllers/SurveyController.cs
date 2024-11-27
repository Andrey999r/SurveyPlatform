using System.Text;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using SurveyPlatform.Models;

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
    public async Task<IActionResult> Create(Survey model, List<string> Questions, List<string> CorrectAnswers, List<string> QuestionTypes)
    {
        if (Questions == null || Questions.Count == 0 || Questions.All(string.IsNullOrWhiteSpace))
        {
            ModelState.AddModelError("Questions", "Добавьте хотя бы один вопрос.");
        }
        if (ModelState.IsValid)
        {
            var formattedQuestions = new List<string>();
            var formattedCorrectAnswers = new List<string>();

            for (int i = 0; i < Questions.Count; i++)
            {
                string question = Questions[i];
                string correctAnswer = string.IsNullOrWhiteSpace(CorrectAnswers[i]) ? "Ручная проверка" : CorrectAnswers[i];

                formattedQuestions.Add($"{question} (Тип: {QuestionTypes[i]})");
                formattedCorrectAnswers.Add(correctAnswer);
            }

            model.Questions = string.Join("\n", formattedQuestions);
            model.CorrectAnswers = string.Join(";", formattedCorrectAnswers);

            _context.Surveys.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        return View(model);
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
        var survey = await _context.Surveys.FindAsync(id);

        if (survey == null)
        {
            return NotFound();
        }

        var questions = survey.Questions.Split('\n');
        var correctAnswers = survey.CorrectAnswers.Split(';');

        var viewModel = questions.Select((question, index) => new
        {
            Question = question,
            CorrectAnswer = correctAnswers[index] == "Ручная проверка" ? "Ответ проверяется вручную автором" : correctAnswers[index]
        });

        ViewBag.SurveyDetails = viewModel;

        return View(survey);
    }

 


}