namespace SurveyPlatform.Models;
using System.ComponentModel.DataAnnotations;


public class Survey
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название опроса.")]
    [StringLength(255)]
    public string Title { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }

    [Required(ErrorMessage = "Добавьте хотя бы один вопрос.")]
    public string Questions { get; set; }

    public string CorrectAnswers { get; set; }

}
