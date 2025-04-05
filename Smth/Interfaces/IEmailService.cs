using Smth.Data;

namespace Smth.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса отправки email-сообщений.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Отправляет приглашение для прохождения опроса по электронной почте.
        /// </summary>
        /// <param name="recipientEmail">Email получателя.</param>
        /// <param name="surveyLink">Ссылка на опрос.</param>
        void SendSurveyInvitation(string recipientEmail, string surveyLink);
    }
}
