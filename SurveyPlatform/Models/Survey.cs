namespace SurveyPlatform.Models;
using System.ComponentModel.DataAnnotations;


public class Survey
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Пожалуйста, введите название опроса")]
    public string Title { get; set; }

    [Required(ErrorMessage = "Пожалуйста, введите описание опроса")]
    public string Description { get; set; }
    public ICollection<Question> Questions { get; set; }

}

