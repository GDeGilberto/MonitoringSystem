using Microsoft.OpenApi.Models;
using System.Reflection;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace API.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Sistema de Monitoreo API",
                    Version = "v1",
                    Description = @"API para el sistema de monitoreo de estaciones y tanques.
                    
Esta API permite gestionar estaciones, inventarios y registros de descargas en el sistema de monitoreo.

## Características principales
- Gestión de estaciones de monitoreo
- Control de inventarios de tanques
- Registro de descargas
- Autenticación mediante JWT
- Sistema de programación de trabajos con Hangfire",
                    Contact = new OpenApiContact
                    {
                        Name = "Gilberto Alvarez",
                        Email = "gilbertoalvarez514@hotmail.com",
                        Url = new Uri("https://github.com/gdegilberto", UriKind.Absolute)
                    },
                    //License = new OpenApiLicense
                    //{
                    //    Name = "Uso interno",
                    //    Url = new Uri("https://example.com/license", UriKind.Absolute)
                    //},
                    //TermsOfService = new Uri("https://example.com/terms", UriKind.Absolute)
                });

                options.EnableAnnotations();

                // Set the comments path for the XmlComments file
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                // Enable XML comments if the file exists
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }

                // Group endpoints by controller
                options.TagActionsBy(api => [api.GroupName ?? api.ActionDescriptor.RouteValues["controller"]]);
                options.DocInclusionPredicate((name, api) => true);

                // Ordenar tags (controladores) alfabéticamente
                options.OrderActionsBy(apiDesc => apiDesc.RelativePath);

                // Configurar el orden personalizado de los controladores mediante TagActionsBy
                options.TagActionsBy(api => 
                {
                    var controllerName = api.GroupName ?? api.ActionDescriptor.RouteValues["controller"];
                    // Definir el orden con un prefijo numérico que será eliminado en la UI
                    return new[] { GetTagOrderPrefix(controllerName) + controllerName };
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"Encabezado de autorización JWT utilizando el esquema Bearer.
                          Escribe 'Bearer' [espacio] y luego tu token en el campo de entrada.
                          Ejemplo: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
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
                        new string[] {}
                    }
                });

                options.CustomSchemaIds(type => type.FullName);
                options.OperationFilter<SwaggerDefaultValues>();
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
        {
            app.UseSwagger(options =>
            {
                options.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    swaggerDoc.Servers = new List<OpenApiServer>
                    {
                        new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}" }
                    };

                    // Reordenar las etiquetas (controladores) según el orden definido
                    var orderedTags = new List<OpenApiTag>();
                    
                    // Ordenar las etiquetas existentes según el prefijo
                    foreach (var tag in swaggerDoc.Tags)
                    {
                        // Remover prefijo de orden si existe (solo para comparación)
                        string tagName = tag.Name;
                        if (tagName.Length > 3 && tagName[0] == '0' && tagName[2] == '_')
                        {
                            tag.Name = tagName.Substring(3);
                        }
                        orderedTags.Add(tag);
                    }
                    
                    swaggerDoc.Tags = orderedTags;
                });
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sistema de Monitoreo API v1");
                c.RoutePrefix = "swagger";
                c.DocumentTitle = "Sistema de Monitoreo - API Documentation";
                c.DocExpansion(DocExpansion.List);
                c.DefaultModelExpandDepth(2);
                c.DefaultModelsExpandDepth(1);
                c.EnableDeepLinking();
                c.DisplayRequestDuration();
                
                // Función para ordenar las operaciones de cada tag (opcional)
                c.DisplayOperationId();
                
                // Ocultar los modelos por defecto en la parte inferior
                c.DefaultModelsExpandDepth(-1);
            });

            return app;
        }

        // Define el orden de los controladores
        private static string GetTagOrderPrefix(string controllerName)
        {
            return controllerName switch
            {
                "Auth" => "01_",           // Primero Autenticación
                "Estaciones" => "02_",     // Segundo Estaciones
                "Inventarios" => "03_",    // Tercero Inventarios
                "Descargas" => "04_",      // Cuarto Descargas
                _ => "99_"                 // El resto al final por orden alfabético
            };
        }
    }

    // Operation filter for adding default values
    public class SwaggerDefaultValues : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
    {
        public void Apply(OpenApiOperation operation, Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext context)
        {
            // Add operation ID based on action name for better client generation
            operation.OperationId = context.MethodInfo.Name;

            // Add tag descriptions
            var controllerName = context.MethodInfo.DeclaringType?.Name.Replace("Controller", "");
            if (controllerName != null && operation.Tags.Count > 0)
            {
                // Elimina cualquier prefijo de ordenación que pudiera haberse aplicado
                string tagName = operation.Tags[0].Name;
                if (tagName.Length > 3 && tagName[0] == '0' && tagName[2] == '_')
                {
                    operation.Tags[0].Name = tagName.Substring(3);
                }
                
                // Asignar la descripción
                operation.Tags[0].Description = GetTagDescription(controllerName);
            }
        }

        private string GetTagDescription(string controllerName)
        {
            return controllerName switch
            {
                "Auth" => "Endpoints para autenticación y gestión de usuarios",
                "Estaciones" => "Gestión de estaciones de monitoreo",
                "Descargas" => "Control y registro de descargas",
                "Inventarios" => "Gestión de inventarios de tanques",
                _ => $"Operaciones relacionadas con {controllerName}"
            };
        }
    }
}