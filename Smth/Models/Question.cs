namespace Smth.Data
{
    /// <summary>
    /// Представляет вопрос, входящий в определённый опрос.
    /// </summary>
    public class Question
    {
        /// <summary>
        /// Уникальный идентификатор вопроса.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Текст вопроса, отображаемый участнику.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Внешний ключ на опрос, к которому относится вопрос.
        /// </summary>
        public int SurveyId { get; set; }

        /// <summary>
        /// Навигационное свойство — опрос, содержащий данный вопрос.
        /// </summary>
        public Survey Survey { get; set; }
    }
}
