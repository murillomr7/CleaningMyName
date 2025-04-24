using CleaningMyName.Api.Extensions;
using CleaningMyName.Api.Middleware;
using CleaningMyName.Api.Security.Handlers;
using CleaningMyName.Api.Security.Requirements;
using CleaningMyName.Application;
using CleaningMyName.Infrastructure;
using CleaningMyName.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices();
builder.Services.AddHealthChecks(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorizationPolicies();

builder.Services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MinimumAge18", policy =>
        policy.Requirements.Add(new MinimumAgeRequirement(18)));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add Polly for resilience
builder.Services.AddPolly();

var app = builder.Build();

// Wait for database to be ready before migrations
//await app.WaitForDatabaseAsync();

// Apply migrations at startup
await app.Services.MigrateDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    
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

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCustomHealthChecks();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
