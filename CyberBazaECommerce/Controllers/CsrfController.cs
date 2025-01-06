using Microsoft.AspNetCore.Mvc;

namespace CyberBazaECommerce.Controllers
{
	[ApiController]
	[Route("api/csrf")]
	public class CsrfController : ControllerBase
	{
		[HttpGet]
		public IActionResult GetCsrfToken()
		{
			var token = Guid.NewGuid().ToString(); // Генерируем уникальный CSRF-токен

			// Set the cookie
			var cookieOptions = new CookieOptions
			{
				HttpOnly = true,
				Secure = true, // Set Secure attribute only for HTTPS
				SameSite = SameSiteMode.None,
				IsEssential = true,
			};

			Response.Cookies.Append("CSRF-TOKEN", token, cookieOptions);
			return Ok(new { Token = token });
		}
	}
}
