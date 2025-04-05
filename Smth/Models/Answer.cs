namespace Smth.Data
{
    /// <summary>
    /// Представляет ответ участника на конкретный вопрос опроса.
    /// </summary>
    public class Answer
    {
        /// <summary>
        /// Уникальный идентификатор ответа.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Текст ответа, введённый участником.
        /// </summary>
        public string ResponseText { get; set; }

        /// <summary>
        /// Внешний ключ — идентификатор вопроса, на который дан ответ.
        /// Может быть null, если связь не установлена.
        /// </summary>
        public int? QuestionId { get; set; }

        /// <summary>
        /// Объект вопроса, к которому относится данный ответ.
        /// </summary>
        public Question Question { get; set; }

        /// <summary>
        /// Внешний ключ — идентификатор участника, который дал ответ.
        /// Может быть null, если связь не установлена.
        /// </summary>
        public int? ParticipantId { get; set; }

        /// <summary>
        /// Объект участника, который дал данный ответ.
        /// </summary>
        public Participant Participant { get; set; }
    }
}
