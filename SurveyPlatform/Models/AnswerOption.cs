namespace SurveyPlatform.Models;

public class AnswerOption
{
    public int Id { get; set; }
    public string Text { get; set; } // Текст варианта ответа

    // Связь с вопросом
    public int QuestionId { get; set; }
    public Question Question { get; set; }
}