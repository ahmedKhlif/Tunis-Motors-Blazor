using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using webappAPI.Data;
using webappAPI.DTOs;
using webappAPI.Models;

namespace webappAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CartController> _logger;

        public CartController(AppDbContext context, ILogger<CartController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/cart
        [HttpGet]
        public async Task<ActionResult<ApiResponse<CartDto>>> GetCart()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated" });

                // Find draft order as cart (status Pending with no customer info)
                var cart = await _context.Orders
                    .Include(o => o.Items)
                    .ThenInclude(oi => oi.CarListing)
                    .FirstOrDefaultAsync(o => o.UserId == userId && string.IsNullOrEmpty(o.CustomerName));

                if (cart == null)
                {
                    return Ok(new ApiResponse<CartDto>
                    {
                        Success = true,
                        Data = new CartDto
                        {
                            CartId = 0,
                            Items = new List<CartItemDto>(),
                            TotalAmount = 0,
                            ItemCount = 0
                        },
                        Message = "Cart is empty"
                    });
                }

                var cartDto = new CartDto
                {
                    CartId = cart.Id,
                    Items = cart.Items.Select(item => new CartItemDto
                    {
                        ItemId = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        ProductImage = item.CarListing?.Image ?? "",
                        Price = item.Price,
                        Quantity = item.Quantity,
                        Total = item.Price * item.Quantity
                    }).ToList(),
                    TotalAmount = cart.TotalAmount,
                    ItemCount = cart.Items.Count
                };

                return Ok(new ApiResponse<CartDto>
                {
                    Success = true,
                    Data = cartDto,
                    Message = "Cart retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting cart: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error retrieving cart" });
            }
        }

        // POST: api/cart/add
        [HttpPost("add")]
        public async Task<ActionResult<ApiResponse<CartItemDto>>> AddToCart([FromBody] AddToCartDto model)
        {
            try
            {
                if (model.Quantity <= 0)
                    return BadRequest(new ApiResponse { Success = false, Message = "Quantity must be greater than 0" });

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated" });

                // Get car listing
                var car = await _context.CarListings.FindAsync(model.ProductId);
                if (car == null)
                    return NotFound(new ApiResponse { Success = false, Message = "Product not found" });

                if (!car.IsApproved)
                    return BadRequest(new ApiResponse { Success = false, Message = "This listing is not approved yet" });

                // Find or create cart order (empty CustomerName = draft cart)
                var cart = await _context.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.UserId == userId && string.IsNullOrEmpty(o.CustomerName));

                if (cart == null)
                {
                    cart = new Order
                    {
                        UserId = userId,
                        CustomerName = "", // Empty = draft cart
                        Email = "",
                        Address = "",
                        PaymentMethod = "",
                        Status = OrderStatus.Pending,
                        TotalAmount = 0,
                        OrderDate = DateTime.UtcNow,
                        Items = new List<OrderItem>()
                    };

                    _context.Orders.Add(cart);
                    await _context.SaveChangesAsync();
                }

                // Check if item already in cart
                var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == model.ProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity += model.Quantity;
                }
                else
                {
                    var cartItem = new OrderItem
                    {
                        OrderId = cart.Id,
                        ProductId = model.ProductId,
                        ProductName = car.Name,
                        Quantity = model.Quantity,
                        Price = car.Price
                    };

                    cart.Items.Add(cartItem);
                }

                // Recalculate total
                cart.TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity);
                _context.Orders.Update(cart);
                await _context.SaveChangesAsync();

                var itemDto = new CartItemDto
                {
                    ItemId = existingItem?.Id ?? cart.Items.Last().Id,
                    ProductId = model.ProductId,
                    ProductName = car.Name,
                    ProductImage = car.Image,
                    Price = car.Price,
                    Quantity = existingItem?.Quantity ?? model.Quantity,
                    Total = (existingItem?.Quantity ?? model.Quantity) * car.Price
                };

                _logger.LogInformation($"Item added to cart by user {userId}");

                return Ok(new ApiResponse<CartItemDto>
                {
                    Success = true,
                    Data = itemDto,
                    Message = "Item added to cart successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding to cart: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error adding item to cart" });
            }
        }

        // DELETE: api/cart/{itemId}
        [HttpDelete("{itemId}")]
        public async Task<ActionResult<ApiResponse>> RemoveFromCart(int itemId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated" });

                var item = await _context.OrderItems
                    .Include(i => i.Order)
                    .FirstOrDefaultAsync(i => i.Id == itemId);

                if (item == null)
                    return NotFound(new ApiResponse { Success = false, Message = "Cart item not found" });

                if (item.Order.UserId != userId)
                    return Forbid();

                _context.OrderItems.Remove(item);

                // Recalculate total
                item.Order.TotalAmount = item.Order.Items
                    .Where(i => i.Id != itemId)
                    .Sum(i => i.Price * i.Quantity);

                // If cart is empty, delete it
                if (item.Order.Items.Count == 1)
                {
                    _context.Orders.Remove(item.Order);
                }
                else
                {
                    _context.Orders.Update(item.Order);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Item removed from cart by user {userId}");

                return Ok(new ApiResponse { Success = true, Message = "Item removed from cart successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error removing from cart: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error removing item from cart" });
            }
        }

        // PUT: api/cart/{itemId}
        [HttpPut("{itemId}")]
        public async Task<ActionResult<ApiResponse<CartItemDto>>> UpdateCartItem(int itemId, [FromBody] UpdateCartItemDto model)
        {
            try
            {
                if (model.Quantity <= 0)
                    return BadRequest(new ApiResponse { Success = false, Message = "Quantity must be greater than 0" });

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated" });

                var item = await _context.OrderItems
                    .Include(i => i.Order)
                    .Include(i => i.CarListing)
                    .FirstOrDefaultAsync(i => i.Id == itemId);

                if (item == null)
                    return NotFound(new ApiResponse { Success = false, Message = "Cart item not found" });

                if (item.Order.UserId != userId)
                    return Forbid();

                item.Quantity = model.Quantity;

                // Recalculate total
                item.Order.TotalAmount = item.Order.Items.Sum(i => i.Price * i.Quantity);

                _context.OrderItems.Update(item);
                _context.Orders.Update(item.Order);
                await _context.SaveChangesAsync();

                var itemDto = new CartItemDto
                {
                    ItemId = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ProductImage = item.CarListing?.Image ?? "",
                    Price = item.Price,
                    Quantity = item.Quantity,
                    Total = item.Price * item.Quantity
                };

                return Ok(new ApiResponse<CartItemDto>
                {
                    Success = true,
                    Data = itemDto,
                    Message = "Cart item updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating cart item: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error updating cart item" });
            }
        }

        // GET: api/cart/count
        [HttpGet("count")]
        public async Task<ActionResult<ApiResponse<int>>> GetCartCount()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Ok(new ApiResponse<int> { Success = true, Data = 0 });

                var count = await _context.Orders
                    .Where(o => o.UserId == userId && string.IsNullOrEmpty(o.CustomerName))
                    .SelectMany(o => o.Items)
                    .CountAsync();

                return Ok(new ApiResponse<int> { Success = true, Data = count });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting cart count: {ex.Message}");
                return Ok(new ApiResponse<int> { Success = true, Data = 0 });
            }
        }

        // DELETE: api/cart/clear
        [HttpDelete("clear")]
        public async Task<ActionResult<ApiResponse>> ClearCart()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated" });

                var cart = await _context.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.UserId == userId && string.IsNullOrEmpty(o.CustomerName));

                if (cart != null)
                {
                    _context.Orders.Remove(cart);
                    await _context.SaveChangesAsync();
                }

                return Ok(new ApiResponse { Success = true, Message = "Cart cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error clearing cart: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Error clearing cart" });
            }
        }
    }
}
