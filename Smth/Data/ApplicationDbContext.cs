namespace Smth.Data;

using Microsoft.EntityFrameworkCore;
/// <summary>
/// Контекст базы данных приложения
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    /// <summary>
    /// Пользователи системы
    /// </summary>
    public DbSet<ApplicationUser> Users { get; set; }
    /// <summary>
    /// Опросы (сущности верхнего уровня)
    /// </summary>
    public DbSet<Survey> Surveys { get; set; }
    /// <summary>
    /// Вопросы в рамках опросов
    /// </summary>
    public DbSet<Question> Questions { get; set; }
    /// <summary>
    /// Участники опросов
    /// </summary>
    public DbSet<Participant> Participants { get; set; }
    /// <summary>
    /// Ответы участников на вопросы
    /// </summary>
    public DbSet<Answer> Answers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Здесь можно добавить конфигурацию отношений между сущностями
    }

}
