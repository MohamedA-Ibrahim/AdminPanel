using Meziantou.Extensions.Logging.InMemory;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers;

[ApiController]
[Route("logs")]
public class LogsController : ControllerBase
{
    private readonly InMemoryLoggerProvider _logProvider;

    public LogsController(InMemoryLoggerProvider logProvider)
    {
        _logProvider = logProvider;
    }

    [HttpGet("messages")]
    public IActionResult GetMessageLogs()
    {
        var logs = _logProvider.Logs.Informations.Where(log => log.Message.Contains("Message with correlationId"));

        return Ok(logs);
    }
}
