using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CyberBazaECommerce
{
	public class AuthorizeCheckOperationFilter : IOperationFilter
	{
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			var hasAuthorizeAttribute = context.MethodInfo.DeclaringType != null && (context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
										 context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any());

			if (hasAuthorizeAttribute)
			{
				operation.Responses.Add("401", new OpenApiResponse { Description = "need to be admin" });
			}

		}
	}
}
