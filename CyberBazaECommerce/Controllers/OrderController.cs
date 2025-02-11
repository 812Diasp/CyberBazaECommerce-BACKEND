using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using CyberBazaECommerce.Services;
using CyberBazaECommerce.Models;
using CyberBazaECommerce.Utils;

namespace CyberBazaECommerce.Controllers
{

	[ApiController]
	[Route("api/orders")]
	[EnableCors("AllowAll")]
	public class OrderController : ControllerBase
	{
		private readonly IOrderService _orderService;
		private readonly CsrfValidator _csrfValidator;

		public OrderController(IOrderService orderService, CsrfValidator csrfValidator)
		{
			_orderService = orderService;
			_csrfValidator = csrfValidator;
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
		{
			var validationResult = _csrfValidator.ValidateCsrfToken();
			if (validationResult is not OkResult)
			{
				return validationResult;
			}

			if (User.Identity is not { IsAuthenticated: true })
			{
				return Unauthorized("User is not authenticated.");
			}

			var userId = User.FindFirst("uid")?.Value;
			var order = new Order
			{
				UserId = userId,
				OrderItems = createOrderDto.OrderItems,
				ShippingAddress = createOrderDto.ShippingAddress,
				PaymentMethod = createOrderDto.PaymentMethod
			};

			var createdOrder = await _orderService.CreateOrderAsync(order);
			return CreatedAtAction(nameof(GetOrderById), new { orderId = createdOrder.Id }, createdOrder);

		}


		[HttpGet("{orderId}")]
		[Authorize]
		public async Task<IActionResult> GetOrderById(string orderId)
		{
			var userId = User.FindFirst("uid")?.Value;

			if (string.IsNullOrEmpty(userId))
			{
				return BadRequest("User ID not found in JWT");
			}
			var order = await _orderService.GetOrderByIdAsync(orderId);
			if (order == null)
			{
				return NotFound();
			}
			if (order.UserId != userId)
			{
				return Unauthorized("You don't have permission to access this order");
			}

			return Ok(order);
		}

		[HttpGet("me")]
		[Authorize]
		public async Task<IActionResult> GetMyOrders()
		{
			var userId = User.FindFirst("uid")?.Value;

			if (string.IsNullOrEmpty(userId))
			{
				return BadRequest("User ID not found in JWT");
			}
			var orders = await _orderService.GetOrdersByUserIdAsync(userId);

			return Ok(orders);
		}

		[HttpPut("{orderId}/status")]
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> UpdateOrderStatus(string orderId, [FromBody] string newStatus)
		{
			var validationResult = _csrfValidator.ValidateCsrfToken();
			if (validationResult is not OkResult)
			{
				return validationResult;
			}

			await _orderService.UpdateOrderStatusAsync(orderId, newStatus);
			return Ok("Status Updated");
		}
	}
}
