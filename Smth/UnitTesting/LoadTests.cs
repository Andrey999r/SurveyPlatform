using System;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using NBomber.Contracts;
using NBomber.CSharp;
using Newtonsoft.Json;
using Xunit;

public class LoadTests
{
    /// <summary>
    /// Этот тест имитирует регистрацию пользователей с нагрузкой 500 запросов в секунду.
    /// Тест выполняется в течение 2 минут.
    /// </summary>
    [Fact]
    public void Register_LoadTest_500RPS()
    {
        // Создаем сценарий для нагрузки "register_load"
        var scenario = Scenario.Create("register_load", async context =>
        {
            // Создаем экземпляр HttpClient для выполнения HTTP-запроса
            using var client = new HttpClient();
            // Формируем уникальный email, используя номер вызова, чтобы избежать дублирования
            var email = $"user_{context.InvocationNumber}@test.com";

            // Сериализуем объект с данными для регистрации в JSON
            var jsonPayload = JsonConvert.SerializeObject(new
            {
                username = $"user_{context.InvocationNumber}",
                email = email,
                password = "Password123!"
            });

            // Создаем содержимое запроса с указанной кодировкой и типом контента
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Отправляем POST-запрос на конечную точку регистрации
            var response = await client.PostAsync("https://localhost:5001/Account/Register", content);

            // Если запрос выполнен успешно, возвращаем Response.Ok(), иначе Response.Fail()
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        // Симуляция нагрузки: 500 запросов в секунду на протяжении 2 минут
        .WithLoadSimulations(
            Simulation.Inject(rate: 500,
                interval: TimeSpan.FromSeconds(1),
                during: TimeSpan.FromMinutes(2))
        );

        // Регистрируем сценарий в NBomber, задаем имя файла отчета и запускаем тест
        NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFileName("load_report")
            .Run();
    }

    /// <summary>
    /// Этот тест имитирует отправку невалидных данных регистрации.
    /// Нагрузка составляет 100 запросов в секунду в течение 1 минуты.
    /// </summary>
    [Fact]
    public void Register_InvalidData_LoadTest()
    {
        // Создаем сценарий для нагрузки "register_invalid_load"
        var scenario = Scenario.Create("register_invalid_load", async context =>
        {
            // Создаем экземпляр HttpClient для выполнения HTTP-запроса
            using var client = new HttpClient();

            // Сериализуем объект с невалидными данными для регистрации в JSON
            var jsonPayload = JsonConvert.SerializeObject(new
            {
                username = "",          // Невалидное значение для имени пользователя
                email = "invalid-email", // Невалидный формат email
                password = "123"         // Слишком простой пароль
            });

            // Создаем содержимое запроса с указанной кодировкой и типом контента
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Отправляем POST-запрос на конечную точку регистрации с невалидными данными
            var response = await client.PostAsync("https://localhost:5001/Account/Register", content);

            // Если запрос выполнен успешно, возвращаем Response.Ok(), иначе Response.Fail()
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        // Симуляция нагрузки: 100 запросов в секунду на протяжении 1 минуты
        .WithLoadSimulations(
            Simulation.Inject(rate: 100,
                interval: TimeSpan.FromSeconds(1),
                during: TimeSpan.FromMinutes(1))
        );

        // Регистрируем сценарий в NBomber, задаем имя файла отчета и запускаем тест
        NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFileName("invalid_load_report")
            .Run();
    }
}
