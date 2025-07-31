using AdminPanel.Models;
using AdminPanel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;
    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Return list of users
    /// </summary>
    [HttpGet("claims")]
    [Authorize]
    public async Task<IActionResult> GetUserClaims(CancellationToken cancellationToken)
    {
        var claims = HttpContext.User.Claims.Select(c => new { c.Type, c.Value });

        return Ok(claims);
    }


    /// <summary>
    /// Return list of users
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<User>), 200)]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _userService.GetUsersAsync(cancellationToken);

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
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
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
    [ProducesResponseType(typeof(User), 201)]
    [ProducesResponseType(typeof(string), 400)]
    public async Task<IActionResult> AddUser(User newUser)
    {
        var result = await _userService.AddAsync(newUser);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Validation failed for adding the user {userName}", newUser.FirstName);

            return BadRequest(result.ToString());
        }


        _logger.LogInformation("User {userName} added successfully.", newUser.FirstName);

        return CreatedAtAction(nameof(GetById), new { id = newUser.Id }, newUser);
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    /// <param name="id">User id</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(string), 400)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var succeeded = await _userService.DeleteAsync(id);
        if (!succeeded)
            return BadRequest("User not found");

        _logger.LogInformation("User {userId} deleted successfully.", id);

        return NoContent();
    }
}
