using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebAPI.Swagger
{
    /// <summary>
    /// Operation filter to add authentication information to the operation summary
    /// </summary>
    public class OperationSummaryFilter : IOperationFilter
    {
        /// <summary>
        /// Applies the filter to the specified operation using the given context.
        /// </summary>
        /// <param name="operation">The operation to apply the filter to.</param>
        /// <param name="context">The current operation filter context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Get authentication requirements
            var declaringTypeAttributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>() ?? Enumerable.Empty<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();
            var hasAuth = declaringTypeAttributes.Any() ||
                          context.MethodInfo.GetCustomAttributes(true).OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any();

            if (!hasAuth)
                return;

            // Get any roles required
            var methodAttributes = context.MethodInfo.GetCustomAttributes(true);
            var typeAttributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true) ?? Array.Empty<object>();
            var authorizeAttributes = methodAttributes
                .Union(typeAttributes)
                .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();

            var requiredRoles = authorizeAttributes
                .Where(attr => !string.IsNullOrEmpty(attr.Roles))
                .SelectMany(attr => attr.Roles?.Split(',') ?? Array.Empty<string>())
                .Distinct()
                .ToList();

            // Simply append [ADMIN] or [AUTH] to the summary
            if (requiredRoles.Any(r => r.Contains("Admin")))
            {
                operation.Summary = $"{operation.Summary} [ADMIN]";
            }
            else if (hasAuth)
            {
                operation.Summary = $"{operation.Summary} [AUTH]";
            }
        }
    }
}
