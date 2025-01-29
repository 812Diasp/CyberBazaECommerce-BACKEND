using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CyberBazaECommerce.Controllers
{
	[ApiController]
	[Route("api/csrf")]
	[EnableCors("AllowAll")]
	public class CsrfController : ControllerBase
	{
		[HttpGet]
		public IActionResult GetCsrfToken()
		{
			var token = Guid.NewGuid().ToString(); // Генерируем уникальный CSRF-токен

			// Set the cookie
			var cookieOptions = new CookieOptions
			{
				HttpOnly = false, // Куки доступна в JS
				SameSite = SameSiteMode.None,
				Secure = true, // Кука доступна только при HTTPS
				IsEssential = true
			};

			Response.Cookies.Append("CSRF-TOKEN", token, cookieOptions);
			return Ok(new { token = token });
		}
	}
}
