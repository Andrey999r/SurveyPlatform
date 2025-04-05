namespace Smth.ViewModel;

public class SurveyDetailsViewModel
{
    public int Id { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public List<ParticipantViewModel> Participants { get; set; } = new();
}

public class ParticipantViewModel
{
    public int Id { get; set; }  // Уникальный идентификатор
    public string ParticipantName { get; set; } = string.Empty;
    public string SurveyName { get; set; } = string.Empty; // Добавляем свойство
    public DateTime CompletedAt { get; set; }


    public string Email { get; set; } = string.Empty;
    public List<AnswerViewModel> Answers { get; set; } = new();
}


public class AnswerViewModel
{
    public string Text { get; set; }
    public string ResponseText { get; set; }
}
public class ThankYouViewModel
{
    public string SurveyName { get; set; }
    public string Email { get; set; }
    public DateTime CompletedAt { get; set; }
}