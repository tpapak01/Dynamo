using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using FluentValidation;
using Dynamo.Business.Data;
using Dynamo.Business.Middleware;
using Dynamo.Business.Models;
using Dynamo.Business.Validator;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DynamoContextConnection") ?? throw new InvalidOperationException("Connection string 'DynamoContextConnection' not found.");

builder.Services.AddDbContext<DynamoContext>(options => options.UseSqlServer(connectionString));

//Additional db-context, apart from the one added above, as an object
DbContextOptionsBuilder<DynamoContext> _optionsBuilder = new DbContextOptionsBuilder<DynamoContext>();
_optionsBuilder.UseSqlServer(connectionString);
DynamoContext dbcontext = new DynamoContext(_optionsBuilder.Options);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Add ApiKey Authentication
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "The API key to access the API",
        Type = SecuritySchemeType.ApiKey,
        Name = "x-api-key",
        In = ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                },
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

builder.Services.AddScoped<IValidator<List<EnergyDataBody>>, RequestValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ApiKeyAuthMiddleware>();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// GET API
app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

// POST API
using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
ILogger logger = factory.CreateLogger("Program");
MyAPIs.Initialize<DynamoContext, HouseAliases>(logger);
MyAPIs.ReceiveEnergyMeasurements(app, "/energydata");
MyAPIs.SendEnergyPredictions(app, "/energypredictions");

// POST REQUEST EVERY 15 MIN TO ELECTI
//new MyWorker(dbcontext);

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}