using Microsoft.AspNetCore.Mvc;

namespace CyberBazaECommerce.Utils
{
	public class CsrfValidator
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ILogger<CsrfValidator> _logger;

		public CsrfValidator(IHttpContextAccessor httpContextAccessor, ILogger<CsrfValidator> logger)
		{
			_httpContextAccessor = httpContextAccessor;
			_logger = logger;
		}

		public IActionResult ValidateCsrfToken()
		{
			var request = _httpContextAccessor.HttpContext.Request;
			string csrfTokenHeader = request.Headers["X-CSRF-Token"].ToString();
			string csrfTokenCookie = request.Cookies["CSRF-TOKEN"];


			if (string.IsNullOrEmpty(csrfTokenHeader))
			{
				_logger.LogWarning("CSRF token is missing in headers.");
				return new BadRequestObjectResult("CSRF token is missing in headers");
			}

			if (string.IsNullOrEmpty(csrfTokenCookie))
			{
				_logger.LogWarning("CSRF token is missing in cookies.");
				return new BadRequestObjectResult("CSRF token is missing in cookies");
			}

			if (csrfTokenHeader != csrfTokenCookie)
			{
				_logger.LogWarning("CSRF tokens mismatch. Header: {HeaderToken}, Cookie: {CookieToken}", csrfTokenHeader, csrfTokenCookie);
				return new BadRequestObjectResult("Invalid CSRF token");
			}


			return new OkResult();
		}

	}

}
