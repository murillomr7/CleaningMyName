using CleaningMyName.Api.Extensions;
using CleaningMyName.Api.Middleware;
using CleaningMyName.Api.Security.Handlers;
using CleaningMyName.Api.Security.Requirements;
using CleaningMyName.Application;
using CleaningMyName.Infrastructure;
using CleaningMyName.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add API services
builder.Services.AddApiServices();

// Add health checks
builder.Services.AddHealthChecks(builder.Configuration);

// Add JWT authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add authorization policies
builder.Services.AddAuthorizationPolicies();

// Register authorization handlers
builder.Services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();

// Add additional policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MinimumAge18", policy =>
        policy.Requirements.Add(new MinimumAgeRequirement(18)));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Apply migrations at startup
await app.Services.MigrateDatabaseAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    
    // Configure Swagger
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CleaningMyName API v1");
        c.RoutePrefix = string.Empty;
    });
}
else
{
    app.UseHsts();
}

// Use custom exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

// Add health checks endpoints
app.UseCustomHealthChecks();

// Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Make Program class public for integration testing
public partial class Program { }
