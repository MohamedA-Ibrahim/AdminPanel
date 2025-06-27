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
}
