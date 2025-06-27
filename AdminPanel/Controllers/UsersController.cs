using AdminPanel.Models;
using AdminPanel.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Return list of users
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<User>), 200)]
    public IActionResult GetUsers()
    {
        var users = _userService.GetUsers();

        return Ok(users);
    }

    /// <summary>
    /// Get user by id
    /// </summary>
    /// <param name="id">User id</param>
    /// <returns>User or 404 error if user was not found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(User), 200)]
    [ProducesResponseType(404)]
    public IActionResult GetById([FromRoute] Guid id)
    {
        var user = _userService.GetById(id);
        if (user is null)
            return NotFound();

        return Ok(user);
    }

    /// <summary>
    /// Add a user
    /// </summary>
    /// <param name="newUser">User details</param>
    /// <returns>The added user</returns>
    [HttpPost]
    [ProducesResponseType(typeof(User), 200)]
    public IActionResult AddUser(User newUser)
    {
        _userService.AddUser(newUser);

        return Ok(newUser);
    }
}
