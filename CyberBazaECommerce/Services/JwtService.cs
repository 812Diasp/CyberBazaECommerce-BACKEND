using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CyberBazaECommerce.Services
{
	public class JwtService
	{
		private readonly IConfiguration _config;

		public JwtService(IConfiguration config)
		{
			_config = config;
		}

		public string GenerateJwtToken(string userId, string username, string role = null) // Изменено
		{
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			var claims = new List<Claim> {
				new Claim(ClaimTypes.Name, username), // Добавляем имя пользователя
				new Claim(ClaimTypes.NameIdentifier, userId),// Добавляем для корзины пользователя
			   new Claim("uid", userId), // Добавляем идентификатор пользователя (наш uid)
                // Добавляем claim для роли, если она есть
            };
			if (!string.IsNullOrEmpty(role))
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}
			else
			{
				claims.Add(new Claim(ClaimTypes.Role, "customer"));
			}


			var token = new JwtSecurityToken(
				issuer: _config["Jwt:Issuer"],
				audience: _config["Jwt:Audience"],
				claims: claims,
				expires: DateTime.Now.AddMinutes(Convert.ToInt32(_config["Jwt:ExpireMinutes"])),
				signingCredentials: credentials);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
