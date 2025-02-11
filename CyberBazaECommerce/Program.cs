using CyberBazaECommerce;
using CyberBazaECommerce.Controllers;
using CyberBazaECommerce.Models;
using CyberBazaECommerce.Services;
using CyberBazaECommerce.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Security.Claims;
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
//��������� csrf
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<CsrfValidator>();
// ������������ ProductService ��� Scoped (���������� ��� ������� �������)
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ILogger<ReviewService>, Logger<ReviewService>>();
//userservice for cart
builder.Services.AddScoped<UserService>();
builder.Services.AddSingleton<IOrderService, OrderService>();
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
			 ClockSkew = TimeSpan.Zero, // ���������� Clock Skew ��� ����� ������� ��������� �������.
		 };
		 options.Events = new JwtBearerEvents
		 {
			 OnTokenValidated = context =>
			 {
				 // �������� �� ������� ���� � claim
				 if (!context.Principal.HasClaim(c => c.Type == ClaimTypes.Role))
				 {
					 context.Fail("You do not have the correct roles.");
				 }
				 return Task.CompletedTask;
			 }
		 };
	 });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "CyberBazaECommerce", Version = "v1" });

	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Description = "JWT Authorization header using the Bearer scheme.",
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT"
	});
	c.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
	{
		new OpenApiSecurityScheme
		{
			Reference = new OpenApiReference
			{
			   Type = ReferenceType.SecurityScheme,
				Id = "Bearer"
			 }
		},
	new string[] { }
	 }
	});
	c.OperationFilter<AuthorizeCheckOperationFilter>(); // Add this line
});

// -- ��������� CORS (���� �����) --
// ������ ���������, ����������� ������� � ������ ������ (�� ������������� ��� production)
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", builder =>
	{
		builder
		   .WithOrigins("http://localhost:5173", "http://localhost:5248","http://localhost:5174") // �������� �� ��� �������� � ������ ������
			.AllowAnyMethod()
			.AllowAnyHeader()
			.AllowCredentials();// ������� ��� ���� �� ����� ���������� ����
	});
});
// -- ����� ��������� CORS --
string redisConnectionString = builder.Configuration.GetConnectionString("RedisConnectionString") ?? "localhost:6379";
//����������� redis
builder.Services.AddStackExchangeRedisCache(options =>
{
	options.Configuration = builder.Configuration.GetConnectionString("RedisConnectionString");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
app.UseSwagger();
app.UseSwaggerUI();
}

app.UseRouting();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
// -- ����� Use CORS --
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();

// ����� ��� �������� MongoDB
public class MongoDbSettings
{
	public string ConnectionString { get; set; }
	public string DatabaseName { get; set; }
}