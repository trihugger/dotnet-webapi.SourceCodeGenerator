using DN.WebApi.Infrastructure.Persistence.Extensions;
using DN.WebApi.Infrastructure.SwaggerFilters;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.IO;
using DN.WebApi.Application.Settings;
using Microsoft.Extensions.Configuration;

namespace DN.WebApi.Infrastructure.Extensions
{
    public static class SwaggerExtensions
    {
        internal static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app, Microsoft.Extensions.Configuration.IConfiguration config)
        {
            if (config.GetValue<bool>("SwaggerSettings:Enable"))
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.DefaultModelsExpandDepth(-1);
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                    options.RoutePrefix = "swagger";
                    options.DisplayRequestDuration();
                    options.DocExpansion(DocExpansion.None);
                });
            }

            return app;
        }

        internal static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration config)
        {
            var settings = services.GetOptions<SwaggerSettings>(nameof(SwaggerSettings));
            if (settings.Enable)
            {
                services.AddSwaggerGen(options =>
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (!assembly.IsDynamic)
                        {
                            string xmlFile = $"{assembly.GetName().Name}.xml";
                            string xmlPath = Path.Combine(baseDirectory, xmlFile);
                            if (File.Exists(xmlPath))
                            {
                                options.IncludeXmlComments(xmlPath);
                            }
                        }
                    }

                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        Description = "Input your Bearer token in this format - Bearer {your token here} to access this API",
                    });

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer",
                                },
                                Scheme = "Bearer",
                                Name = "Bearer",
                                In = ParameterLocation.Header,
                            }, new List<string>()
                        },
                    });

                    options.MapType<TimeSpan>(() => new OpenApiSchema
                    {
                        Type = "string",
                        Nullable = true,
                        Pattern = @"^([0-9]{1}|(?:0[0-9]|1[0-9]|2[0-3])+):([0-5]?[0-9])(?::([0-5]?[0-9])(?:.(\d{1,9}))?)?$",
                        Example = new OpenApiString("02:00:00")
                    });

                    options.OperationFilter<AddTenantIdFilter>();
                });
            }

            return services;
        }
    }
}