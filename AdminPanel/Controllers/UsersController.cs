using AdminPanel.Models;
using AdminPanel.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AdminPanel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;
    private readonly IConfiguration _configuration;
    private readonly ServiceBusClient _serviceBusClient;
    public UsersController(IUserService userService, ILogger<UsersController> logger, IConfiguration configuration, ServiceBusClient serviceBusClient)
    {
        _userService = userService;
        _logger = logger;
        _configuration = configuration;
        _serviceBusClient = serviceBusClient;
    }

    /// <summary>
    /// Return list of users
    /// </summary>
    [HttpGet("claims")]
    [Authorize]
    public Task<IActionResult> GetUserClaims()
    {
        var claims = HttpContext.User.Claims.Select(c => new { c.Type, c.Value });

        return Task.FromResult<IActionResult>(Ok(claims));
    }


    /// <summary>
    /// Return list of users
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<User>), 200)]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersFilter filter, CancellationToken cancellationToken)
    {
        var result = await _userService.GetUsersAsync(filter, cancellationToken);
        var users = result.Data;
        
        var cacheHeader = result.CacheHit ? "HIT" : "MISS";
        Response.Headers.Append("X-Cache", cacheHeader);

        return Ok(users);
    }

    /// <summary>
    /// Get user by id
    /// </summary>
    /// <param name="id">User id</param>
    /// <param name="cancellationToken"></param>
    /// <returns>User or 404 error if user was not found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(User), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _userService.GetByIdAsync(id, cancellationToken);
        var user = result.Data;

        var cacheHeader = result.CacheHit ? "HIT" : "MISS";
        Response.Headers.Append("X-Cache", cacheHeader);

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
    [Authorize]
    public async Task<IActionResult> AddUser(User newUser)
    {
        var serializedUser = JsonSerializer.Serialize(newUser);

        var sender = _serviceBusClient.CreateSender("queue.1");
        var message = new ServiceBusMessage(serializedUser);

        await sender.SendMessageAsync(message);

        return Accepted();
     }

    /// <summary>
    /// Delete a user
    /// </summary>
    /// <param name="id">User id</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(string), 400)]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var succeeded = await _userService.DeleteAsync(id);
        if (!succeeded)
            return BadRequest("User not found");

        _logger.LogInformation("User {userId} deleted successfully.", id);

        return NoContent();
    }
}
