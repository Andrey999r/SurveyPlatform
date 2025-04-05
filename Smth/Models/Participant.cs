namespace Smth.Data;

using System.Collections.Generic;

public class Participant
{
    public int Id { get; set; }
    public string Email { get; set; }
    // Дата прохождения опроса (время UTC + 3 часа для московского времени)

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow.AddHours(3);
    public string ParticipantName { get; set; }
    public int SurveyId { get; set; }
    // Навигационное свойство к опросу
    public Survey Survey { get; set; }

    // Коллекция ответов участника

    public List<Answer> Answers { get; set; } = new List<Answer>();
}
