﻿using AdminPanel.Models;

namespace AdminPanel.Services;

public interface IUserService
{
    User? GetById(Guid id);
    List<User> GetUsers();
    void AddUser(User user);
    bool DeleteUser(Guid id);
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

    public List<User> GetUsers()
    {
        return _users;

    }

    public User? GetById(Guid id)
    {
        return _users.FirstOrDefault(u => u.Id == id);
    }

    public void AddUser(User user)
    {
        _users.Add(user);
    }

    public bool DeleteUser(Guid id)
    {
        var user = GetById(id);
        if (user is null)
            return false;

        _users.Remove(user);

        return true;
    }
}
