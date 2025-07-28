using AdminPanel.Data;
using AdminPanel.Models;
using AdminPanel.Validators;
using Microsoft.EntityFrameworkCore;

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
    private readonly AppDbContext _dbContext;

    public UserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<User>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Result> AddAsync(User user)
    {
        var validator = new UserValidator();
        var result = await validator.ValidateAsync(user);
        if(!result.IsValid)
            return new Result(false, result.ToString());

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        return new Result(true);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if (user is null)
            return false;

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
