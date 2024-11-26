using System.Text;
using Microsoft.AspNetCore.Authorization;
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

    // GET: /Survey/Create
    [Authorize] // Требуется авторизация
    public IActionResult Create()
    {
        return View();
    }
    // POST: /Survey/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize] // Требуется авторизация
    public async Task<IActionResult> Create(Survey survey)
    {
        if (ModelState.IsValid)
        {
            _context.Add(survey);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(survey);
    }

    // GET: /Survey/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var survey = await _context.Surveys.FirstOrDefaultAsync(m => m.Id == id);
        if (survey == null)
            return NotFound();

        return View(survey);
    }

    // POST: /Survey/Delete/5

    [HttpPost, ActionName("DeleteConfirmed")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var survey = await _context.Surveys.FindAsync(id);
        if (survey != null)
        {
            _context.Surveys.Remove(survey);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }
    // GET: /Survey/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var survey = await _context.Surveys
            .Include(s => s.Questions) // Подключаем связанные вопросы
            .ThenInclude(q => q.AnswerOptions) // Подключаем варианты ответов
            .FirstOrDefaultAsync(m => m.Id == id);

        if (survey == null)
        {
            return NotFound();
        }

        return View(survey);
    }
}