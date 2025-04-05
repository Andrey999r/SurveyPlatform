namespace Smth.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// Представляет опрос, созданный пользователем.
    /// </summary>
    public class Survey
    {
        /// <summary>
        /// Уникальный идентификатор опроса.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название опроса.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание опроса, отображаемое перед его началом.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Внешний ключ — идентификатор пользователя, создавшего опрос.
        /// </summary>
        public int ApplicationUserId { get; set; }

        /// <summary>
        /// Навигационное свойство — владелец (создатель) опроса.
        /// </summary>
        public ApplicationUser Owner { get; set; }

        /// <summary>
        /// Коллекция вопросов, включённых в данный опрос.
        /// </summary>
        public List<Question> Questions { get; set; } = new List<Question>();

        /// <summary>
        /// Коллекция участников, прошедших опрос.
        /// </summary>
        public List<Participant> Participants { get; set; } = new List<Participant>();
    }
}
