using System.Reflection;
using CleaningMyName.Api.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;

namespace CleaningMyName.Api.Extensions;

public static class ApiExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        // Add API versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version"));
        });

        // Configure swagger versioning
        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // Configure Swagger
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CleaningMyName API",
                Version = "v1",
                Description = "A RESTful API built with .NET 7 and Clean Architecture",
                Contact = new OpenApiContact
                {
                    Name = "CleaningMyName Support",
                    Email = "support@cleaningmyname.com",
                    Url = new Uri("https://cleaningmyname.com/support")
                }
            });

            // Add JWT authentication to Swagger
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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

            // Use XML comments for API documentation
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    public static IApplicationBuilder UseApiConfiguration(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseSwagger();

        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                // Create a separate middleware for Swagger UI
                // Direct implementation to avoid extension method dependency
                context.Response.StatusCode = 200;
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(@"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>CleaningMyName API</title>
                    <meta charset='utf-8' />
                    <meta name='viewport' content='width=device-width, initial-scale=1'>
                    <link href='/swagger/swagger-ui.css' rel='stylesheet' type='text/css' />
                </head>
                <body>
                    <div id='swagger-ui'></div>
                    <script src='/swagger/swagger-ui-bundle.js'></script>
                    <script src='/swagger/swagger-ui-standalone-preset.js'></script>
                    <script>
                        window.onload = function() {
                            var ui = SwaggerUIBundle({
                                url: '/swagger/v1/swagger.json',
                                dom_id: '#swagger-ui',
                                deepLinking: true,
                                presets: [
                                    SwaggerUIBundle.presets.apis,
                                    SwaggerUIStandalonePreset
                                ],
                                layout: 'StandaloneLayout'
                            });
                            window.ui = ui;
                        }
                    </script>
                </body>
                </html>");
                return;
            }

            await next();
        });

        return app;
    }
}

