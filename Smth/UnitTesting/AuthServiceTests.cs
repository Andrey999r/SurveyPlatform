using Microsoft.EntityFrameworkCore;
using Moq;
using Smth.Data;
using Smth.Services;
using Xunit;

public class AuthServiceTests
{
    private ApplicationDbContext GetInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }
    [Fact]
    public void Register_InvalidEmail_ThrowsException()
    {
        // Arrange
        var context = GetInMemoryContext("TestDb_InvalidEmail");
        var authService = new AuthService(context);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            authService.Register("user1", "invalid-email", "Password123!")
        );
    }

    [Fact]
    public void Register_ShortPassword_ThrowsException()
    {
        // Arrange
        var context = GetInMemoryContext("TestDb_ShortPassword");
        var authService = new AuthService(context);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            authService.Register("user1", "test@mail.com", "123")
        );
    }

    [Fact]
    public void Login_InvalidPassword_ThrowsException()
    {
        // Arrange
        var context = GetInMemoryContext("TestDb_InvalidPassword");
        var authService = new AuthService(context);
        authService.Register("user1", "test@mail.com", "Password123!");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            authService.Login("user1", "WrongPassword!")
        );
    }

    [Fact]
    public void Login_NonExistentUser_ThrowsException()
    {
        // Arrange
        var context = GetInMemoryContext("TestDb_NonExistentUser");
        var authService = new AuthService(context);

        // Act & Assert
        // Проверка ошибки при попытке входа несуществующего пользователя

        Assert.Throws<InvalidOperationException>(() =>
            authService.Login("ghost", "Password123!")
        );
    }
    [Fact]
    public void Register_ValidData_CreatesUser()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_Register")
            .Options;

        using var context = new ApplicationDbContext(options);
        var authService = new AuthService(context);

        // Act
        var user = authService.Register("testUser", "test@mail.com", "Password123!");

        // Assert
        Assert.NotNull(user);
        Assert.Equal("test@mail.com", user.Email);
        Assert.True(BCrypt.Net.BCrypt.Verify("Password123!", user.PasswordHash));
    }

    [Fact]
    public void Register_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_Duplicate")
            .Options;

        using var context = new ApplicationDbContext(options);
        var authService = new AuthService(context);
        // Создание первого пользователя

        authService.Register("user1", "duplicate@mail.com", "Password123!");

        // Act & Assert
        // Проверка исключения при повторной регистрации с тем же email
        Assert.Throws<InvalidOperationException>(() =>
            authService.Register("user2", "duplicate@mail.com", "Password456!")
        );
    }
}