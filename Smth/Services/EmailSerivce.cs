using System;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Smth.Data;
using Smth.Interfaces;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Конструктор, получающий объект конфигурации для доступа к настройкам электронной почты.
    /// </summary>
    /// <param name="configuration">Объект конфигурации приложения</param>
    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Отправляет приглашение пройти опрос на указанный email.
    /// </summary>
    /// <param name="recipientEmail">Email получателя</param>
    /// <param name="surveyLink">Ссылка на опрос</param>
    public void SendSurveyInvitation(string recipientEmail, string surveyLink)
    {
        try
        {
            // Получаем настройки SMTP сервера из конфигурационного файла
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderPassword = _configuration["EmailSettings:SenderPassword"];

            // Создаем клиент SMTP с заданным сервером и портом
            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                // Устанавливаем учетные данные для аутентификации
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                // Включаем SSL для безопасного соединения
                EnableSsl = true,
                // Устанавливаем таймаут в 20 секунд для отправки письма
                Timeout = 20000
            };

            // Формируем письмо с заданными параметрами
            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail), // Адрес отправителя
                Subject = "Приглашение пройти опрос",  // Тема письма
                Body = $"Пройдите опрос по ссылке: {surveyLink}", // Текст письма с ссылкой
                IsBodyHtml = true // Разрешаем HTML в теле письма
            };

            // Добавляем email получателя
            mail.To.Add(recipientEmail);

            // Отправляем письмо через SMTP клиент
            client.Send(mail);

            // Выводим сообщение об успешной отправке письма
            Console.WriteLine($"Письмо успешно отправлено на {recipientEmail}");
        }
        catch (SmtpException smtpEx)
        {
            // Логгируем ошибку SMTP и пробрасываем исключение с сообщением
            Console.WriteLine("Ошибка SMTP: " + smtpEx.Message);
            throw new Exception("Ошибка SMTP: " + smtpEx.Message);
        }
        catch (Exception ex)
        {
            // Логгируем общую ошибку отправки письма и пробрасываем исключение с сообщением
            Console.WriteLine("Ошибка отправки письма: " + ex.Message);
            throw new Exception("Ошибка отправки письма: " + ex.Message);
        }
    }
}
