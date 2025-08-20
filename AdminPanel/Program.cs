using AdminPanel.Data;
using AdminPanel.Middlewares;
using AdminPanel.Models;
using AdminPanel.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(options =>
    {
        options.WithOrigins("http://localhost:4200")
               .AllowAnyHeader()
               .WithMethods("GET", "POST", "PUT", "DELETE");
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

builder.Services.AddScoped<IUserService, UserService>();
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
   return ConnectionMultiplexer.Connect(new ConfigurationOptions()
    {
        EndPoints = { "localhost:6379" },
        AbortOnConnectFail = false

    });
});
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
