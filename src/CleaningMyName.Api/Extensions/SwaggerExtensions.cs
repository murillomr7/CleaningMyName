using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace CleaningMyName.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            // Basic API information
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CleaningMyName API",
                Version = "v1",
                Description = "A RESTful API built with .NET 7 and Clean Architecture principles",
                Contact = new OpenApiContact
                {
                    Name = "CleaningMyName Team",
                    Email = "support@cleaningmyname.com",
                    Url = new Uri("https://cleaningmyname.com/contact")
                },
                License = new OpenApiLicense
                {
                    Name = "Use under MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Add JWT authentication support to Swagger
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.\r\n\r\n" +
                              "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                              "Example: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Configure XML comments
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Add examples for requests and responses
            options.ExampleFilters();

            // Order the actions
            options.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
        });

        // Register the Swagger examples from this assembly
        services.AddSwaggerExamplesFromAssemblyOf<Program>();

        return services;
    }

    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "api-docs/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/api-docs/v1/swagger.json", "CleaningMyName API v1");
            options.RoutePrefix = "api-docs";
            options.DocumentTitle = "CleaningMyName API Documentation";
            options.DefaultModelsExpandDepth(2);
            options.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.ShowExtensions();
        });

        return app;
    }
}
