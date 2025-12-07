using Microsoft.EntityFrameworkCore;
using webappAPI.Data;
using webappAPI.DTOs;
using webappAPI.Models;

namespace webappAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderService> _logger;
        private readonly IEmailService _emailService;

        public OrderService(AppDbContext context, ILogger<OrderService> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<OrderDto> GetOrderByIdAsync(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .ThenInclude(oi => oi.CarListing)
                    .FirstOrDefaultAsync(o => o.Id == id);

                return order != null ? MapToDto(order) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting order: {ex.Message}");
                return null;
            }
        }

        public async Task<PaginatedDto<OrderDto>> GetUserOrdersAsync(string userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Orders.Where(o => o.UserId == userId);
                var totalCount = await query.CountAsync();

                var orders = await query
                    .Include(o => o.Items)
                    .ThenInclude(oi => oi.CarListing)
                    .OrderByDescending(o => o.OrderDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PaginatedDto<OrderDto>
                {
                    Data = orders.Select(MapToDto).ToList(),
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user orders: {ex.Message}");
                return new PaginatedDto<OrderDto>();
            }
        }

        public async Task<PaginatedDto<OrderDto>> GetAllOrdersAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Orders;
                var totalCount = await query.CountAsync();

                var orders = await query
                    .Include(o => o.Items)
                    .ThenInclude(oi => oi.CarListing)
                    .OrderByDescending(o => o.OrderDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PaginatedDto<OrderDto>
                {
                    Data = orders.Select(MapToDto).ToList(),
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all orders: {ex.Message}");
                return new PaginatedDto<OrderDto>();
            }
        }

        public async Task<OrderDto> CreateOrderAsync(string userId, CreateOrderDto model)
        {
            try
            {
                decimal totalAmount = 0;
                var order = new Order
                {
                    CustomerName = model.CustomerName,
                    Email = model.Email,
                    Address = model.Address,
                    PaymentMethod = model.PaymentMethod,
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending
                };

                if (model.Items != null && model.Items.Any())
                {
                    foreach (var item in model.Items)
                    {
                        var orderItem = new OrderItem
                        {
                            ProductId = item.ProductId,
                            ProductName = item.ProductName ?? "",
                            Quantity = item.Quantity,
                            Price = item.Price
                        };
                        order.Items.Add(orderItem);
                        totalAmount += item.Price * item.Quantity;
                    }
                }

                order.TotalAmount = totalAmount;
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Send order confirmation email to customer
                try
                {
                    await _emailService.SendOrderConfirmationAsync(
                        model.Email,
                        model.CustomerName,
                        order.Id,
                        totalAmount
                    );
                    _logger.LogInformation($"[OrderService] Order confirmation email sent to {model.Email}");
                }
                catch (Exception emailEx)
                {
                    _logger.LogError($"[OrderService] Failed to send order confirmation email: {emailEx.Message}");
                    // Don't fail the order creation if email fails
                }

                return await GetOrderByIdAsync(order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating order: {ex.Message}");
                return null;
            }
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(int id, UpdateOrderStatusDto model, string updatedBy)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .ThenInclude(oi => oi.CarListing)
                    .ThenInclude(cl => cl.Seller)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                    return null;

                var oldStatus = order.Status;

                if (Enum.TryParse<OrderStatus>(model.Status, out var status))
                {
                    order.Status = status;
                    order.StatusUpdatedAt = DateTime.UtcNow;
                    order.StatusUpdatedBy = updatedBy;
                    if (!string.IsNullOrEmpty(model.Notes))
                        order.Notes = model.Notes;

                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();

                    // Send email notification to buyer if status changed
                    if (oldStatus != status)
                    {
                        try
                        {
                            await _emailService.SendOrderStatusUpdateAsync(
                                order.Email,
                                order.CustomerName,
                                order.Id,
                                status.ToString()
                            );
                            _logger.LogInformation($"[OrderService] Status update email sent to customer {order.Email}");
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogError($"[OrderService] Failed to send status update email to buyer: {emailEx.Message}");
                        }

                        // Send congratulatory email to seller if order is delivered (car sold)
                        if (status == OrderStatus.Delivered && oldStatus != OrderStatus.Delivered)
                        {
                            try
                            {
                                // Send email to seller for each car in the order
                                foreach (var item in order.Items)
                                {
                                    var carListing = await _context.CarListings
                                        .Include(c => c.Seller)
                                        .FirstOrDefaultAsync(c => c.ProductId == item.ProductId);

                                    if (carListing?.Seller != null && !string.IsNullOrEmpty(carListing.Seller.Email))
                                    {
                                        await _emailService.SendCarSoldNotificationAsync(
                                            carListing.Seller.Email,
                                            carListing.Seller.UserName,
                                            carListing.Name,
                                            carListing.Price
                                        );
                                        _logger.LogInformation($"[OrderService] Car sold notification email sent to seller {carListing.Seller.Email} for car {carListing.Name}");
                                    }
                                }
                            }
                            catch (Exception emailEx)
                            {
                                _logger.LogError($"[OrderService] Failed to send car sold notification email to seller: {emailEx.Message}");
                            }
                        }
                    }
                }

                return await GetOrderByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating order status: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CancelOrderAsync(int id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null || !order.CanBeCancelled)
                    return false;

                order.Status = OrderStatus.Cancelled;
                order.StatusUpdatedAt = DateTime.UtcNow;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error cancelling order: {ex.Message}");
                return false;
            }
        }

        public async Task<PaginatedDto<OrderDto>> FilterOrdersAsync(string? status, string? search, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Orders.AsQueryable();

                if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var orderStatus))
                    query = query.Where(o => o.Status == orderStatus);

                if (!string.IsNullOrEmpty(search))
                    query = query.Where(o =>
                        o.CustomerName.Contains(search) ||
                        o.Email.Contains(search));

                var totalCount = await query.CountAsync();
                var orders = await query
                    .Include(o => o.Items)
                    .ThenInclude(oi => oi.CarListing)
                    .OrderByDescending(o => o.OrderDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PaginatedDto<OrderDto>
                {
                    Data = orders.Select(MapToDto).ToList(),
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error filtering orders: {ex.Message}");
                return new PaginatedDto<OrderDto>();
            }
        }

        private OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                Email = order.Email,
                Address = order.Address,
                PaymentMethod = order.PaymentMethod,
                TotalAmount = order.TotalAmount,
                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                Notes = order.Notes,
                StatusUpdatedAt = order.StatusUpdatedAt,
                StatusUpdatedBy = order.StatusUpdatedBy,
                UserId = order.UserId,
                Items = order.Items?.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    CreatedAt = oi.CreatedAt
                }).ToList()
            };
        }
    }
}
