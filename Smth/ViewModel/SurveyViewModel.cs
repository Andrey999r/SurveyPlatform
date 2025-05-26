namespace Smth.ViewModel;

/// <summary>
/// Модель представления для отображения детальной информации об опросе
/// </summary>
public class SurveyDetailsViewModel
{
    public int Id { get; set; }         // Уникальный идентификатор опроса
    public string Name { get; set; }    // Название опроса
    public string Description { get; set; } // Описание опроса
    public List<ParticipantViewModel> Participants { get; set; } = new(); // Список участников опроса
}

/// <summary>
/// Модель представления участника опроса
/// </summary>
public class ParticipantViewModel
{
    public int Id { get; set; }         // Уникальный идентификатор участника
    public string ParticipantName { get; set; } = string.Empty; // Имя участника
    public string SurveyName { get; set; } = string.Empty; // Название связанного опроса
    public DateTime CompletedAt { get; set; } // Дата и время прохождения опроса

    public string Email { get; set; } = string.Empty; // Email участника
    public List<AnswerViewModel> Answers { get; set; } = new(); // Список ответов участника
    public bool CanEdit { get; set; } = false;
}

/// <summary>
/// Модель представления ответа на вопрос
/// </summary>
public class AnswerViewModel
{
    public int    Id   { get; set; }          // ← добавили Id строки Answer

    public string Text { get; set; }     // Текст вопроса (из связанной сущности Question)
    public string ResponseText { get; set; } // Текст ответа участника
}

/// <summary>
/// Модель представления для страницы благодарности после прохождения опроса
/// </summary>
public class ThankYouViewModel
{
    public string SurveyName { get; set; }    // Название пройденного опроса
    public string Email { get; set; }         // Email участника
    public DateTime CompletedAt { get; set; } // Дата и время завершения опроса
}
    public class EditSurveyViewModel
    {
        public int    Id          { get; set; }
        public string Name        { get; set; }
        public string Description { get; set; }

        public List<QuestionEditModel> Questions { get; set; } = new();
    }

    /// <summary>Парочка «Id + Text» одного вопроса.</summary>
    public class QuestionEditModel
    {
        public int    Id   { get; set; }
        public string Text { get; set; }
    }