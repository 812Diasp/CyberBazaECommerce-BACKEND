using CyberBazaECommerce.Models;
using CyberBazaECommerce.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register services
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

// ---  ��������� MongoDB  ---
// ������ ��������� �� appsettings.json
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));

// ������������ IMongoClient ��� ��������
builder.Services.AddSingleton<IMongoClient>(provider =>
{
var settings = provider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
return new MongoClient(settings.ConnectionString);
});
// ������������ IMongoDatabase ��� ��������
builder.Services.AddSingleton<IMongoDatabase>(provider =>
{
var client = provider.GetRequiredService<IMongoClient>();
var settings = provider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
return client.GetDatabase(settings.DatabaseName);
});
// ������������ ProductService ��� Scoped (���������� ��� ������� �������)
builder.Services.AddScoped<ProductService>();
// --- ����� �������� MongoDB ---


// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
 .AddJwtBearer(options =>
{
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

options.TokenValidationParameters = new TokenValidationParameters
{
ValidateIssuer = true,
ValidateAudience = true,
ValidateLifetime = true,
ValidateIssuerSigningKey = true,
ValidIssuer = jwtSettings["Issuer"],
ValidAudience = jwtSettings["Audience"],
IssuerSigningKey = new SymmetricSecurityKey(key),
ClockSkew = TimeSpan.Zero // ���������� Clock Skew ��� ����� ������� ��������� �������.
};
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -- ��������� CORS (���� �����) --
// ������ ���������, ����������� ������� � ������ ������ (�� ������������� ��� production)
builder.Services.AddCors(options =>
{
options.AddPolicy("AllowAll",
	builder => builder.AllowAnyOrigin()
	.AllowAnyMethod()
	.AllowAnyHeader());
});
// -- ����� ��������� CORS --

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
app.UseSwagger();
app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// -- Use CORS (���� �����) --
app.UseCors("AllowAll");
// -- ����� Use CORS --

app.UseAuthentication(); // Add Authentication Middleware
app.UseAuthorization(); // Add Authorization Middleware

app.MapControllers();

app.Run();

// ����� ��� �������� MongoDB
public class MongoDbSettings
{
	public string ConnectionString { get; set; }
	public string DatabaseName { get; set; }
}