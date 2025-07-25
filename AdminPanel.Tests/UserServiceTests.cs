using AdminPanel.Models;
using AdminPanel.Services;

namespace AdminPanel.Tests;

public class UserServiceTests
{
    private readonly IUserService _userService;

    public UserServiceTests()
    {
        _userService = new UserService();
    }

    [Fact]
    public async Task GetUsersAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var users = await _userService.GetUsersAsync(cancellationToken);

        // Assert
        Assert.NotNull(users);
        Assert.NotEmpty(users);
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
        var user = await _userService.GetByIdAsync(newUser.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(user);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var user = await _userService.GetByIdAsync(id, CancellationToken.None);

        // Assert
        Assert.Null(user);
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
        var users = await _userService.GetUsersAsync(CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(users);
        Assert.NotEmpty(users);
        Assert.Contains(users, u => u.Id == newUser.Id && u.FirstName == newUser.FirstName);
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
        var users = await _userService.GetUsersAsync(CancellationToken.None);

        Assert.True(success);
        Assert.DoesNotContain(users, u => u.Id == newUser.Id );
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
}
