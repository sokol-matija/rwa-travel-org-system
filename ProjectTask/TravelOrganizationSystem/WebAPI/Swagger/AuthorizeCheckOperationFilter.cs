using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebAPI.Swagger
{
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Get endpoint metadata for controller and action
            var hasAuthorize =
                context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() == true ||
                context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

            if (!hasAuthorize) return;

            // Get any roles required
            var authorizeAttributes = context.MethodInfo.GetCustomAttributes(true)
                .Union(context.MethodInfo.DeclaringType?.GetCustomAttributes(true) ?? Array.Empty<object>())
                .OfType<AuthorizeAttribute>();

            var requiredRoles = authorizeAttributes
                .Where(attr => !string.IsNullOrEmpty(attr.Roles))
                .SelectMany(attr => attr.Roles!.Split(','))
                .Distinct()
                .ToList();

            // Add JWT authentication requirement to operation
            operation.Security.Add(new OpenApiSecurityRequirement
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
                    new string[] { }
                }
            });

            // Add authentication & roles info to description
            var authDescription = "**REQUIRES AUTHENTICATION**";
            if (requiredRoles.Any())
            {
                authDescription += $"\n\nRequired role(s): {string.Join(", ", requiredRoles)}";
            }

            operation.Description = string.IsNullOrEmpty(operation.Description)
                ? authDescription
                : $"{operation.Description}\n\n{authDescription}";
        }
    }
}
