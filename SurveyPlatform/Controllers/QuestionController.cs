using SurveyPlatform.Models;

namespace SurveyPlatform.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class QuestionController : Controller
{
    private readonly ApplicationDbContext _context;

    public QuestionController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Question/Create/{surveyId}
    public IActionResult Create(int surveyId)
    {
        ViewBag.SurveyId = surveyId;
        return View();
    }

    // POST: /Question/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Question question, List<string> answerOptions)
    {
        if (ModelState.IsValid)
        {
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            // Добавляем варианты ответа, если они заданы
            if (question.Type != QuestionType.TextInput && answerOptions != null)
            {
                foreach (var option in answerOptions)
                {
                    if (!string.IsNullOrWhiteSpace(option))
                    {
                        _context.AnswerOptions.Add(new AnswerOption
                        {
                            Text = option,
                            QuestionId = question.Id
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Survey", new { id = question.SurveyId });
        }
        ViewBag.SurveyId = question.SurveyId;
        return View(question);
    }
}
