namespace Smth.Data
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Представляет участника, прошедшего опрос.
    /// </summary>
    public class Participant
    {
        /// <summary>
        /// Уникальный идентификатор участника.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Email участника (используется для идентификации или связи).
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Имя участника.
        /// </summary>
        public string ParticipantName { get; set; }

        /// <summary>
        /// Время завершения опроса. 
        /// По умолчанию устанавливается как текущее UTC-время + 3 часа (Московское время).
        /// </summary>
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow.AddHours(3);

        /// <summary>
        /// Внешний ключ на опрос, в котором участвовал участник.
        /// </summary>
        public int SurveyId { get; set; }

        /// <summary>
        /// Навигационное свойство — опрос, к которому относится участник.
        /// </summary>
        public Survey Survey { get; set; }

        /// <summary>
        /// Список ответов участника на вопросы опроса.
        /// </summary>
        public List<Answer> Answers { get; set; } = new List<Answer>();
    }
}
