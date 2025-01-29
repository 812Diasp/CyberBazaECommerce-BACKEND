using CyberBazaECommerce.Models;
using CyberBazaECommerce.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CyberBazaECommerce.Controllers
{
	[ApiController]
	[Route("api/auth")]
	[EnableCors("AllowAll")]
	public class AuthController : ControllerBase
	{
		private readonly MongoDbService _mongoDbService;
		private readonly JwtService _jwtService;
		private readonly IPasswordHasher<User> _passwordHasher;
		private readonly EmailService _emailService;
		private readonly IConfiguration _configuration;

		public AuthController(MongoDbService mongoDbService, JwtService jwtService, IPasswordHasher<User> passwordHasher, IConfiguration configuration)
		{
			_mongoDbService = mongoDbService;
			_jwtService = jwtService;
			_passwordHasher = passwordHasher;
			_configuration = configuration;
			_emailService = GetEmailService();

		}
		private EmailService GetEmailService()
		{
			var emailSettings = _configuration.GetSection("SmtpSettings");
			return new EmailService(
			  emailSettings["Server"],
			  int.Parse(emailSettings["Port"]),
			  emailSettings["Username"],
			  emailSettings["Password"],
			  emailSettings["EmailFrom"]
		   );
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterModel model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var existingUser = await _mongoDbService.GetUserByEmailAsync(model.Email);

			if (existingUser != null)
			{
				return BadRequest("Email already in use");
			}


			var user = new User
			{
				Username = model.Username,
				Email = model.Email,
				Role = "customer"
			};

			user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);
			// Обновление кода в базе данных
			var confirmationCode = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
			user.ConfirmationCode = confirmationCode;
			user.ConfirmationCodeExpiration = DateTime.UtcNow.AddMinutes(10);
			await _mongoDbService.CreateUserAsync(user);


			// Отправка email с кодом подтверждения
			await SendConfirmationEmail(user.Email, confirmationCode);

			// Здесь код для создания и выдачи JWT токена
			var token = _jwtService.GenerateJwtToken(user.Id, user.Username, user.Role);

			return Ok(new
			{
				token = token,
				user = new { username = user.Username, email = user.Email }
			});
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginModel model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var user = await _mongoDbService.GetUserByEmailAsync(model.Email);

			if (user == null)
			{
				return Unauthorized("Invalid email or password");
			}

			var passwordVerification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
			if (passwordVerification == PasswordVerificationResult.Failed)
			{
				return Unauthorized("Invalid email or password");
			}

			// Генерация кода подтверждения
			var confirmationCode = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
			// Обновляем код и время экспирации
			user.ConfirmationCode = confirmationCode;
			user.ConfirmationCodeExpiration = DateTime.UtcNow.AddMinutes(10);
			await _mongoDbService.UpdateUserAsync(user);


			await SendConfirmationEmail(user.Email, confirmationCode);


			return Ok("Confirmation code sent to your email");
		}


		[HttpPost("login/confirm")]
		public async Task<IActionResult> ConfirmLogin([FromBody] ConfirmLoginModel model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var user = await _mongoDbService.GetUserByEmailAsync(model.Email);

			if (user == null)
			{
				return Unauthorized("Invalid email or code");
			}
			if (user.ConfirmationCode != model.ConfirmationCode)
			{
				return BadRequest("Invalid confirmation code");
			}
			if (user.ConfirmationCodeExpiration < DateTime.UtcNow)
			{
				return BadRequest("Confirmation code expired");
			}
			// Если все проверки прошли успешно, то генерируем токен
			var token = _jwtService.GenerateJwtToken(user.Id, user.Username, user.Role); // Use User.Username for token
			user.ConfirmationCode = null; // Сбрасываем код
			user.ConfirmationCodeExpiration = DateTime.MinValue;
			await _mongoDbService.UpdateUserAsync(user);

			return Ok(new
			{
				token = token,
				user = new { username = user.Username, email = user.Email }
			});
		}
		[HttpPost("forgot-password/request")]
		public async Task<IActionResult> ForgotPasswordRequest([FromBody] ForgotPasswordModel model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var user = await _mongoDbService.GetUserByEmailAsync(model.Email);
			if (user == null)
			{
				return NotFound();
			}
			// Генерация кода для востановления пароля
			var resetCode = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
			user.ConfirmationCode = resetCode;
			user.ConfirmationCodeExpiration = DateTime.UtcNow.AddMinutes(10);
			await _mongoDbService.UpdateUserAsync(user);

			// Отправка email c кодом для востановления
			await _emailService.SendResetPasswordEmailAsync(model.Email, resetCode);
			return Ok("Reset password code was send to your email!");
		}

		[HttpPost("forgot-password/reset")]
		public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var user = await _mongoDbService.GetUserByEmailAsync(model.Email);

			if (user == null)
			{
				return Unauthorized("Invalid email or code");
			}
			if (user.ConfirmationCode != model.ResetCode)
			{
				return BadRequest("Invalid reset password code");
			}
			if (user.ConfirmationCodeExpiration < DateTime.UtcNow)
			{
				return BadRequest("Reset code expired");
			}
			// Обновляем пароль
			user.PasswordHash = _passwordHasher.HashPassword(user, model.NewPassword);
			user.ConfirmationCode = null;
			user.ConfirmationCodeExpiration = DateTime.MinValue;
			await _mongoDbService.UpdateUserAsync(user);

			return Ok("Password changed successfully");
		}

		private async Task SendConfirmationEmail(string email, string code)
		{
			var subject = "Confirmation Code";
			var body = $"<h1>Your code for login is:</h1><h1><strong>{code}</strong></h1>";
			await _emailService.SendEmailAsync(email, subject, body);
		}
	}
	public class LoginModel
	{
		public string Email { get; set; }
		public string Password { get; set; }
	}
	public class ForgotPasswordModel
	{
		[Required]
		public string Email { get; set; }
	}
	public class ResetPasswordModel
	{
		[Required]
		public string Email { get; set; }

		[Required]
		public string ResetCode { get; set; }
		[Required]
		public string NewPassword { get; set; }
	}
	public class ConfirmLoginModel
	{
		[Required]
		public string Email { get; set; }
		[Required]
		public string ConfirmationCode { get; set; }
	}
	public class RegisterModel
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string Email { get; set; }
	}
}