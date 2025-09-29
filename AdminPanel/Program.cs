using AdminPanel.Data;
using AdminPanel.Middlewares;
using AdminPanel.Models;
using AdminPanel.Models.Settings;
using AdminPanel.Services;
using Meziantou.Extensions.Logging.InMemory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

using var loggerProvider = new InMemoryLoggerProvider();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddProvider(loggerProvider);
builder.Services.AddSingleton(loggerProvider);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(options =>
    {
        options.WithOrigins("http://localhost:4200")
               .AllowAnyHeader()
               .WithMethods("GET", "POST", "PUT", "DELETE")
               .WithExposedHeaders("X-Cache");
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(sg =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    sg.IncludeXmlComments(xmlPath);
});

builder.Services.AddScoped<IUserService, DbUserService>();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options
    .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    .UseSeeding((context, _) =>
    {
        var existingData = context.Set<User>().Any();
        if (existingData)
            return;
        
        var users = new List<User>
        {
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
        };

        context.Set<User>().AddRange(users);
        context.SaveChanges();

    });
});

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.Authority = "https://localhost:5001";
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString");

    return ConnectionMultiplexer.Connect(new ConfigurationOptions()
    {
        EndPoints = { connectionString },
        AbortOnConnectFail = false,
       

    });
});

var serviceBusConnection = builder.Configuration.GetConnectionString("AzureServiceBus");

builder.Services.AddAzureClients(builder =>
{
    builder.AddServiceBusClient(serviceBusConnection);
});

builder.Services.AddHostedService<AddUserQueueService>();

builder.Services.Configure<FeatureConfig>(builder.Configuration.GetSection("Features"));
var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();
app.UseAuthorization();

app.MapControllers();

app.Run();
