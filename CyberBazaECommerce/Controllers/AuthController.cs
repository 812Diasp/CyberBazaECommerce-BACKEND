using CyberBazaECommerce.Models;
using CyberBazaECommerce.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CyberBazaECommerce.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly MongoDbService _mongoDbService;
		private readonly JwtService _jwtService;
		private readonly IPasswordHasher<User> _passwordHasher;

		public AuthController(MongoDbService mongoDbService, JwtService jwtService, IPasswordHasher<User> passwordHasher)
		{
			_mongoDbService = mongoDbService;
			_jwtService = jwtService;
			_passwordHasher = passwordHasher;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterModel model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var existingUser = await _mongoDbService.GetUserByUsernameAsync(model.Username);

			if (existingUser != null)
			{
				return BadRequest("Username already in use");
			}

			existingUser = await _mongoDbService.GetUserByEmailAsync(model.Email);

			if (existingUser != null)
			{
				return BadRequest("Email already in use");
			}

			var user = new User
			{
				Username = model.Username,
				Email = model.Email,
			};

			user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);
			await _mongoDbService.CreateUserAsync(user);

			return Ok("User Created");
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginModel model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var user = await _mongoDbService.GetUserByUsernameAsync(model.Username);

			if (user == null)
			{
				return Unauthorized("Invalid username or password");
			}

			var passwordVerification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

			if (passwordVerification == PasswordVerificationResult.Failed)
			{
				return Unauthorized("Invalid username or password");
			}

			var token = _jwtService.GenerateJwtToken(user.Username);

			return Ok(new { Token = token });
		}
	}

	public class LoginModel
	{
		public string Username { get; set; }
		public string Password { get; set; }
	}

	public class RegisterModel
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string Email { get; set; }
	}
}
