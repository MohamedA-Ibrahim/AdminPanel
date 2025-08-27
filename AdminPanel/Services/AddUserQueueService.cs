
using AdminPanel.Models;
using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace AdminPanel.Services;

public class AddUserQueueService : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AddUserQueueService> _logger;
    public AddUserQueueService(ServiceBusClient client, IServiceScopeFactory scopeFactory, ILogger<AddUserQueueService> logger)
    {
        _client = client;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var processor = _client.CreateProcessor("queue.1");

        processor.ProcessMessageAsync += MessageHandler;
        processor.ProcessErrorAsync += ErrorHandler;

        await processor.StartProcessingAsync(stoppingToken);
    }

    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        var body = args.Message.Body.ToString();
        var user = JsonSerializer.Deserialize<User>(body);
        if (user == null)
            return;

        using var scope = _scopeFactory.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var result = await userService.AddAsync(user);
        if(!result.Succeeded)
        {
            _logger.LogError("Failed to add user: {userName} with error: {errorMessage}", user.FirstName, result.Message);
            return;
        }

        _logger.LogInformation("User {userName} added successfully", user.FirstName);

        await args.CompleteMessageAsync(args.Message);
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }

}
