using AdminPanel.Data;
using AdminPanel.Models;
using AdminPanel.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using StackExchange.Redis;

namespace AdminPanel.Tests;

public class UserServiceTests
{
    private readonly IUserService _userService;

    public UserServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

       var context = new AppDbContext(options);

        // Mock Redis
        var redisMock = new Mock<IConnectionMultiplexer>();
        redisMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                 .Returns(Mock.Of<IDatabase>());

        // Mock IConfiguration
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Redis:Enabled", "false"}
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();


        _userService = new UserService(context, redisMock.Object, configuration);
    }

    [Fact]
    public async Task GetUsersAsync_ShouldReturnAllUsers()
    {
        // Act
        var result = await _userService.GetUsersAsync(new GetUsersFilter(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "user@gmail.com",
            Phone = "0512354564"
        };

        await _userService.AddAsync(newUser);

        // Act
        var result = await _userService.GetByIdAsync(newUser.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal(newUser.Id, result.Data!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var user = await _userService.GetByIdAsync(id, CancellationToken.None);

        // Assert
        Assert.Null(user.Data);
    }

    [Fact]
    public async Task AddAsync_ShouldAddUser()
    {
        // Arrange
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "user@gmail.com",
            Phone = "0512354564"
        };

        // Act
        var result = await _userService.AddAsync(newUser);

        // Assert
        var users = await _userService.GetUsersAsync(new GetUsersFilter(), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotEmpty(users.Data);
        Assert.Contains(users.Data, u => u.Id == newUser.Id);
    }

    [Fact]
    public async Task AddAsync_WhenEmailIsInvaid_ShouldReturnError()
    {
        // Arrange
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "invalid-email",
            Phone = "0512354564"
        };

        // Act
        var result = await _userService.AddAsync(newUser);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserExists_ShouldDeleteUser()
    {
        // Arrange
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "user@gmail.com",
            Phone = "0512354564"
        };
        await _userService.AddAsync(newUser);

        // Act
        var success = await _userService.DeleteAsync(newUser.Id);

        // Assert
        var result = await _userService.GetUsersAsync(new GetUsersFilter(), CancellationToken.None);
        var users = result.Data;

        Assert.True(success);
        Assert.DoesNotContain(users, u => u.Id == newUser.Id);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserDoesNotExist_ShouldFail()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var success = await _userService.DeleteAsync(id);

        // Assert
        Assert.False(success);
    }

    private static List<User> GetFakeUserList()
    {
        return
        [
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "J.D@gmail.com",
                Phone = "123-456-7890"
            },
            new User
            {
                Id =Guid.NewGuid(),
                FirstName = "Mark",
                LastName = "Luther",
                Email = "M.L@gmail.com",
                Phone = "123-456-7890"
            }
        ];
    }
}
