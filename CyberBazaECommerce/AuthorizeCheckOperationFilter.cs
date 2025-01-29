using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace CyberBazaECommerce
{
	public class AuthorizeCheckOperationFilter : IOperationFilter
	{
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			operation.Description = "For all POST, PUT, and DELETE queries, a CSRF token is required. You can obtain it from route /api/csrf.\n\n" + operation.Description;
			if (string.IsNullOrEmpty(operation.Description))
			{
				operation.Description = "For all POST, PUT, and DELETE queries, a CSRF token is required. You can obtain it from route /api/csrf";
			}
			var authorizeAttributes = context.MethodInfo.GetCustomAttributes<AuthorizeAttribute>(true)
			  .Union(context.MethodInfo.DeclaringType?.GetCustomAttributes<AuthorizeAttribute>(true) ?? Enumerable.Empty<AuthorizeAttribute>());

			if (authorizeAttributes.Any())
			{

				var requiresAdminRole = authorizeAttributes.Any(attr => attr.Roles != null && attr.Roles.Contains("admin"));
				// Проверка, требуется ли роль администратора

				if (requiresAdminRole)
				{
					// Add a parameter indicating that admin role is required
					operation.Parameters.Add(new OpenApiParameter
					{
						Name = "Admin Role Required",
						In = ParameterLocation.Header,
						Description = "This method requires the Admin role.",
						Required = false,
						Schema = new OpenApiSchema { Type = "string" },
						AllowEmptyValue = true
					});
				}

				// Добавляем описание ответа 401 с информацией о роли
				var unauthorizedResponse = new OpenApiResponse { Description = "Unauthorized" };
				if (authorizeAttributes.Any(a => !string.IsNullOrEmpty(a.Roles)))
				{
					unauthorizedResponse.Description = $"Unauthorized. Requires role: {string.Join(", ", authorizeAttributes.Select(a => a.Roles))}";
				}
				operation.Responses.Add("401", unauthorizedResponse);


				// Добавляем описание ответа 403 (запрещено)
				operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });
			}
		}
	}
}
