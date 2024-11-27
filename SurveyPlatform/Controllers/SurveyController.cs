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
    public async Task<IActionResult> Create(Survey survey)
    {
        if (ModelState.IsValid)
        {
            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = survey.Id });
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
   
    public IActionResult AddQuestion(int id)
    {
        var survey = _context.Surveys.Find(id);
        if (survey == null) return NotFound();

        ViewBag.SurveyId = id;
        return View();
    }

    public async Task<IActionResult> Details(int id)
    {
        var survey = await _context.Surveys
            .FirstOrDefaultAsync(s => s.Id == id); // Загружаем только опрос без вопросов

        if (survey == null)
        {
            return NotFound();
        }

        return View(survey); // Передаём модель Survey в представление
    }
  


}