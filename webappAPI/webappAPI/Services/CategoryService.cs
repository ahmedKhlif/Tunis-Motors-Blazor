using Microsoft.EntityFrameworkCore;
using webappAPI.Data;
using webappAPI.DTOs;
using webappAPI.Models;

namespace webappAPI.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(AppDbContext context, ILogger<CategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                return category != null ? MapToDto(category) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting category: {ex.Message}");
                return null;
            }
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _context.Categories
                    .OrderBy(c => c.CategoryName)
                    .ToListAsync();

                var categoryDtos = new List<CategoryDto>();
                foreach (var category in categories)
                {
                    var productCount = await _context.CarListings
                        .CountAsync(c => c.CategoryId == category.CategoryId);
                    
                    var dto = MapToDto(category);
                    dto.ProductCount = productCount;
                    categoryDtos.Add(dto);
                }

                return categoryDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all categories: {ex.Message}");
                return new List<CategoryDto>();
            }
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto model)
        {
            try
            {
                var category = new Category
                {
                    CategoryName = model.CategoryName,
                    Description = model.Description,
                    CreatedAt = DateTime.UtcNow
                };

                // Handle base64 image if provided
                if (!string.IsNullOrEmpty(model.Image) && model.Image.StartsWith("data:image"))
                {
                    try
                    {
                        // Extract base64 data
                        var base64Data = model.Image.Split(',')[1];
                        var imageBytes = Convert.FromBase64String(base64Data);

                        // Determine file extension from MIME type
                        var mimeType = model.Image.Split(',')[0].Split(':')[1].Split(';')[0];
                        var extension = mimeType switch
                        {
                            "image/jpeg" => ".jpg",
                            "image/jpg" => ".jpg",
                            "image/png" => ".png",
                            "image/gif" => ".gif",
                            _ => ".jpg"
                        };

                        // Generate unique filename
                        var fileName = $"{Guid.NewGuid()}{extension}";
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "categories");

                        // Ensure directory exists
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var filePath = Path.Combine(uploadsFolder, fileName);

                        // Save file
                        await File.WriteAllBytesAsync(filePath, imageBytes);

                        category.Image = $"/images/categories/{fileName}";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error saving image: {ex.Message}");
                        // Continue without image if there's an error
                    }
                }
                else if (!string.IsNullOrEmpty(model.Image))
                {
                    // If it's already a path, use it as is
                    category.Image = model.Image;
                }

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return MapToDto(category);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating category: {ex.Message}");
                return null;
            }
        }

        public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto model)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    return null;

                category.CategoryName = model.CategoryName;
                category.Description = model.Description;

                // Handle base64 image if provided
                if (!string.IsNullOrEmpty(model.Image) && model.Image.StartsWith("data:image"))
                {
                    try
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(category.Image))
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", category.Image.TrimStart('/'));
                            if (File.Exists(oldImagePath))
                            {
                                File.Delete(oldImagePath);
                            }
                        }

                        // Extract base64 data
                        var base64Data = model.Image.Split(',')[1];
                        var imageBytes = Convert.FromBase64String(base64Data);

                        // Determine file extension from MIME type
                        var mimeType = model.Image.Split(',')[0].Split(':')[1].Split(';')[0];
                        var extension = mimeType switch
                        {
                            "image/jpeg" => ".jpg",
                            "image/jpg" => ".jpg",
                            "image/png" => ".png",
                            "image/gif" => ".gif",
                            _ => ".jpg"
                        };

                        // Generate unique filename
                        var fileName = $"{Guid.NewGuid()}{extension}";
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "categories");

                        // Ensure directory exists
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var filePath = Path.Combine(uploadsFolder, fileName);

                        // Save file
                        await File.WriteAllBytesAsync(filePath, imageBytes);

                        category.Image = $"/images/categories/{fileName}";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error saving image: {ex.Message}");
                        // Keep existing image if there's an error
                    }
                }
                else if (!string.IsNullOrEmpty(model.Image))
                {
                    // If it's already a path, use it as is
                    category.Image = model.Image;
                }

                category.UpdatedAt = DateTime.UtcNow;

                _context.Categories.Update(category);
                await _context.SaveChangesAsync();

                return MapToDto(category);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating category: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    return false;

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting category: {ex.Message}");
                return false;
            }
        }

        private CategoryDto MapToDto(Category category)
        {
            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                Image = category.Image,
                ProductCount = 0, // Default, will be updated in GetAllCategoriesAsync
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }
    }
}
