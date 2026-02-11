using BOG.API.BackgroundServices;
using BOG.API.Converters;
using BOG.API.Extensions;
using BOG.Integration.Extensions;
using BOG.DbModel;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services following Dependency Inversion Principle
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIntegrationServices(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Use camelCase for JSON property names (JavaScript/TypeScript convention)
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = null;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new NullableDateOnlyJsonConverter());
    });

// Register background services
builder.Services.AddHostedService<CompletionDeadlineCheckerService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Initialize database in development
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        try
        {
            logger.LogInformation("Initializing InMemory database with seed data...");

            // EnsureCreated applies all HasData() seed data from OnModelCreating
            dbContext.Database.EnsureCreated();

            // Log seed data statistics
            logger.LogInformation("InMemory database initialized successfully:");
            logger.LogInformation("  - Roles: {Count}", dbContext.Roles.Count());
            logger.LogInformation("  - Courts: {Count}", dbContext.Courts.Count());
            logger.LogInformation("  - Request Statuses: {Count}", dbContext.RequestStatuses.Count());
            logger.LogInformation("  - Plaintiff Types: {Count}", dbContext.PlaintiffTypes.Count());
            logger.LogInformation("  - Defendant Types: {Count}", dbContext.DefendantTypes.Count());
            logger.LogInformation("  - Representative Types: {Count}", dbContext.RepresentativeTypes.Count());
            logger.LogInformation("  - Identity Types: {Count}", dbContext.IdentityTypes.Count());
            logger.LogInformation("  - Data Sources: {Count}", dbContext.DataSources.Count());
            logger.LogInformation("  - Attachment Types: {Count}", dbContext.AttachmentTypes.Count());
            logger.LogInformation("  - Regions: {Count}", dbContext.Regions.Count());
            logger.LogInformation("  - Cities: {Count}", dbContext.Cities.Count());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize database");
            throw;
        }
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// Only use HTTPS redirection in production to avoid issues with HTTP-only development
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.Run();

// Make Program class public for WebApplicationFactory in tests
public partial class Program { }
