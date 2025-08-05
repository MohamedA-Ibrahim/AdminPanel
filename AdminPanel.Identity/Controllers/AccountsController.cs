using AdminPanel.Identity.Models;
using Duende.IdentityServer;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdminPanel.Identity.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private IIdentityServerInteractionService InteractionService { get; }
    private IServerUrls ServerUrls { get; }
    private SignInManager<ApplicationUser> SignInManager { get; }
    private readonly ITokenService _tokenService;
    public AccountsController(IIdentityServerInteractionService interactionService, IServerUrls serverUrls, SignInManager<ApplicationUser> signInManager, ITokenService tokenService)
    {
        InteractionService = interactionService;
        ServerUrls = serverUrls;
        SignInManager = signInManager;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.IsPersistent, false);
        if (!result.Succeeded)
            return BadRequest();

        var token = new Token(IdentityServerConstants.TokenTypes.AccessToken)
        {
            Issuer = "https://localhost:5001",
            Lifetime = (int)TimeSpan.FromDays(1).TotalSeconds,
            CreationTime = DateTime.UtcNow,
            ClientId = "pat.client",
            Claims = new List<Claim>
            {
                new Claim("client_id", "pat.client"),
                new Claim("sub", User.GetSubjectId())
            },
            AccessTokenType = AccessTokenType.Jwt
        };

        var tokenString = await _tokenService.CreateSecurityTokenAsync(token);

        return Ok(new { token = tokenString, userName= model.UserName});
    }

    public record LoginRequestModel(string UserName, string Password, string? ReturnUrl, bool IsPersistent = false);
}
