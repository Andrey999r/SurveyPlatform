namespace SurveyPlatform.Models;

public class Question
{
    public int Id { get; set; }
    public string Text { get; set; } // Текст вопроса
    public QuestionType Type { get; set; } // Тип вопроса (см. перечисление ниже)

    // Связь с опросом
    public int SurveyId { get; set; }
    public Survey Survey { get; set; }

    // Список возможных вариантов ответа (если это выбор)
    public ICollection<AnswerOption> AnswerOptions { get; set; }
}

// Тип вопроса
public enum QuestionType
{
    TextInput, // Участник вводит ответ вручную
    SingleChoice, // Один из предложенных вариантов
    MultipleChoice // Несколько вариантов (чекбоксы)
}