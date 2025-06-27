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

    [HttpGet]
    [ProducesResponseType(typeof(List<User>), 200)]
    public IActionResult GetUsers()
    {
        var users = _userService.GetUsers();

        return Ok(users);
    }

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
}
