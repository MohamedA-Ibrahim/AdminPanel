
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
        {
            await args.DeadLetterMessageAsync(args.Message, "DeserializationFailed", "Failed to deserialize user object");
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        try
        {

            var result = await userService.AddAsync(user);
            if (!result.Succeeded)
            {
                throw new Exception(result.Message);
            }

            _logger.LogInformation("User {userName} added successfully", user.FirstName);

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            int deliveryCount = args.Message.DeliveryCount;

            if (deliveryCount < 3)
            {
                _logger.LogWarning(ex, "Processing failed for user {userName}. Retrying... Attempt {deliveryCount}", user?.FirstName, deliveryCount);
                await args.AbandonMessageAsync(args.Message);
                return;
            }

            _logger.LogError(ex, "Adding user {userName} failed {deliveryCount} times. Terminating.", user?.FirstName, deliveryCount);

            await args.DeadLetterMessageAsync(
                args.Message,
                "MaxDeliveryAttempts",
                $"Processing failed after {deliveryCount} attempts: {ex.Message}"
            );
        }
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }

}
