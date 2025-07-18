using AdminPanel.Models;
using AdminPanel.Validators;

namespace AdminPanel.Services;

public interface IUserService
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<User>> GetUsersAsync(CancellationToken cancellationToken);
    Task<Result> AddAsync(User user);
    Task<bool> DeleteAsync(Guid id);
}

public class UserService : IUserService
{
    private static readonly List<User> _users =
    [
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Alice",
                LastName = "Johnson",
                Email = "alice.johnson@example.com",
                Phone = "01123456789"
            },
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Omar",
                LastName = "Hassan",
                Email = "omar.hassan@example.com",
                Phone = "01234567890"
            },
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Fatima",
                LastName = "Yousef",
                Email = "fatima.yousef@example.com",
                Phone = "01098765432"
            },
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Phone = "01555555555"
            },
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Sara",
                LastName = "Ahmed",
                Email = "sara.ahmed@example.com",
                Phone = "01299887766"
            },
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Khaled",
                LastName = "Nabil",
                Email = "khaled.nabil@example.com",
                Phone = "01011223344"
            },
    ];

    public async Task<List<User>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        return _users;

    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _users.FirstOrDefault(u => u.Id == id);
    }

    public async Task<Result> AddAsync(User user)
    {
        var validator = new UserValidator();
        var result = await validator.ValidateAsync(user);
        if(!result.IsValid)
            return new Result(false, result.ToString());

        _users.Add(user);

        return new Result(true);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if (user is null)
            return false;

        _users.Remove(user);

        return true;
    }
}
