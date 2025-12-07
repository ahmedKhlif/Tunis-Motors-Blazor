using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using webappAPI.DTOs;
using webappAPI.Services;

namespace webappAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, IPaymentService paymentService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound(new ApiResponse { Success = false, Message = "Order not found" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
                return Forbid();

            return Ok(new ApiResponse<OrderDto> { Success = true, Data = order });
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedDto<OrderDto>>>> GetUserOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var orders = await _orderService.GetUserOrdersAsync(userId, page, pageSize);

            return Ok(new ApiResponse<PaginatedDto<OrderDto>> { Success = true, Data = orders });
        }

        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<PaginatedDto<OrderDto>>>> GetAllOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var orders = await _orderService.GetAllOrdersAsync(page, pageSize);
            return Ok(new ApiResponse<PaginatedDto<OrderDto>> { Success = true, Data = orders });
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder([FromBody] CreateOrderDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid input" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var order = await _orderService.CreateOrderAsync(userId, model);

            if (order == null)
                return BadRequest(new ApiResponse { Success = false, Message = "Failed to create order" });

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, 
                new ApiResponse<OrderDto> { Success = true, Data = order });
        }

        [HttpPost("checkout")]
        public async Task<ActionResult<ApiResponse<OrderDto>>> Checkout([FromBody] CheckoutDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid checkout data" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                // Create order from cart
                var createOrderDto = new CreateOrderDto
                {
                    CustomerName = model.CustomerName,
                    Email = model.Email,
                    Address = model.Address,
                    PaymentMethod = model.PaymentMethod,
                    TotalAmount = model.TotalAmount
                };

                var order = await _orderService.CreateOrderAsync(userId, createOrderDto);

                if (order == null)
                    return BadRequest(new ApiResponse { Success = false, Message = "Failed to process checkout" });

                _logger.LogInformation($"Order created successfully: {order.Id} for user {userId}");

                return Ok(new ApiResponse<OrderDto> 
                { 
                    Success = true, 
                    Data = order, 
                    Message = "Order placed successfully" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Checkout error: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred during checkout" });
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var order = await _orderService.UpdateOrderStatusAsync(id, model, userId);

            if (order == null)
                return BadRequest(new ApiResponse { Success = false, Message = "Failed to update order status" });

            return Ok(new ApiResponse<OrderDto> { Success = true, Data = order });
        }

        [HttpPost("{id}/process-payment")]
        public async Task<ActionResult<ApiResponse<object>>> ProcessPayment(int id, [FromBody] ProcessPaymentDto model)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                    return NotFound(new ApiResponse { Success = false, Message = "Order not found" });

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (order.UserId != userId)
                    return Forbid();

                if (order.PaymentMethod.Equals("stripe", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(model.PaymentIntentId))
                        return BadRequest(new ApiResponse { Success = false, Message = "Payment intent ID is required for Stripe payments" });

                    var paymentConfirmed = await _paymentService.ConfirmPayment(model.PaymentIntentId);
                    if (!paymentConfirmed)
                        return BadRequest(new ApiResponse { Success = false, Message = "Payment confirmation failed" });

                    // Update order status to paid
                    await _orderService.UpdateOrderStatusAsync(id, new UpdateOrderStatusDto { Status = "Paid" }, userId);
                }
                else
                {
                    // For bank transfer and cash on delivery, mark as pending
                    await _orderService.UpdateOrderStatusAsync(id, new UpdateOrderStatusDto { Status = "Pending" }, userId);
                }

                return Ok(new ApiResponse<object> 
                { 
                    Success = true, 
                    Message = "Payment processed successfully",
                    Data = new { OrderId = id, Status = order.PaymentMethod.Equals("stripe", StringComparison.OrdinalIgnoreCase) ? "Paid" : "Pending" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Payment processing error: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Payment processing failed" });
            }
        }

        [HttpPost("{id}/create-payment-intent")]
        public async Task<ActionResult<ApiResponse<object>>> CreatePaymentIntent(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                    return NotFound(new ApiResponse { Success = false, Message = "Order not found" });

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (order.UserId != userId)
                    return Forbid();

                if (!order.PaymentMethod.Equals("stripe", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new ApiResponse { Success = false, Message = "Payment intent can only be created for Stripe payments" });

                var clientSecret = await _paymentService.CreatePaymentIntent(order.TotalAmount, "usd");
                
                return Ok(new ApiResponse<object> 
                { 
                    Success = true, 
                    Data = new { ClientSecret = clientSecret, OrderId = id }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment intent for order {OrderId}: {Message}", id, ex.Message);
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(new ApiResponse { Success = false, Message = $"Failed to create payment intent: {errorMessage}" });
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<ApiResponse>> CancelOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound(new ApiResponse { Success = false, Message = "Order not found" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (order.UserId != userId)
                return Forbid();

            var result = await _orderService.CancelOrderAsync(id);
            if (!result)
                return BadRequest(new ApiResponse { Success = false, Message = "Cannot cancel this order" });

            return Ok(new ApiResponse { Success = true, Message = "Order cancelled successfully" });
        }

        [HttpGet("admin/filter")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<PaginatedDto<OrderDto>>>> FilterOrders(
            [FromQuery] string? status = null, 
            [FromQuery] string? search = null, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            var orders = await _orderService.FilterOrdersAsync(status, search, page, pageSize);
            return Ok(new ApiResponse<PaginatedDto<OrderDto>> { Success = true, Data = orders });
        }
    }
}
