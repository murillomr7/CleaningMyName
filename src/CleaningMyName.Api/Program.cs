using CleaningMyName.Api.Extensions;
using CleaningMyName.Api.Middleware;
using CleaningMyName.Api.Security.Handlers;
using CleaningMyName.Api.Security.Requirements;
using CleaningMyName.Application;
using CleaningMyName.Infrastructure;
using CleaningMyName.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices();
builder.Services.AddHealthChecks(builder.Configuration);

builder.Services.AddAuthorizationPolicies();
builder.Services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MinimumAge18", policy =>
        policy.Requirements.Add(new MinimumAgeRequirement(18)));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddPolly();

var app = builder.Build();

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
app.MapSwagger().AllowAnonymous();

//await app.Services.MigrateDatabaseAsync();

app.Run();

public partial class Program { }
