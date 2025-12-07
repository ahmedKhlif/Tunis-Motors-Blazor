using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using webappAPI.DTOs;
using webappAPI.Services;
using webappAPI.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace webappAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [HttpPost("upload-image")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<FileUploadResponseDto>>> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ApiResponse { Success = false, Message = "No file provided" });

            var allowed = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowed.Contains(file.ContentType.ToLower()))
                return BadRequest(new ApiResponse { Success = false, Message = "Unsupported file type" });

            const long maxSize = 5 * 1024 * 1024; // 5MB
            if (file.Length > maxSize)
                return BadRequest(new ApiResponse { Success = false, Message = "File exceeds 5MB limit" });

            try
            {
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (string.IsNullOrEmpty(extension))
                {
                    extension = file.ContentType switch
                    {
                        "image/jpeg" => ".jpg",
                        "image/jpg" => ".jpg",
                        "image/png" => ".png",
                        "image/gif" => ".gif",
                        _ => ".jpg"
                    };
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                var relativeDir = Path.Combine("images", "categories");
                var saveDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativeDir);
                if (!Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);

                var filePath = Path.Combine(saveDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = $"/{relativeDir.Replace('\\','/')}/{fileName}";
                var response = new FileUploadResponseDto
                {
                    Success = true,
                    Message = "Image uploaded successfully",
                    FilePath = relativePath,
                    FileName = fileName,
                    FileSize = file.Length
                };
                return Ok(new ApiResponse<FileUploadResponseDto> { Success = true, Data = response, Message = response.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading category image");
                return StatusCode(500, new ApiResponse { Success = false, Message = "Internal server error while uploading image" });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(new ApiResponse<List<CategoryDto>> { Success = true, Data = categories });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategory(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound(new ApiResponse { Success = false, Message = "Category not found" });

            return Ok(new ApiResponse<CategoryDto> { Success = true, Data = category });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromBody] CreateCategoryDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid input" });

            var category = await _categoryService.CreateCategoryAsync(model);
            if (category == null)
                return BadRequest(new ApiResponse { Success = false, Message = "Failed to create category" });

            return CreatedAtAction(nameof(GetCategory), new { id = category.CategoryId },
                new ApiResponse<CategoryDto> { Success = true, Data = category });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> UpdateCategory(int id, [FromBody] UpdateCategoryDto model)
        {
            var category = await _categoryService.UpdateCategoryAsync(id, model);
            if (category == null)
                return NotFound(new ApiResponse { Success = false, Message = "Category not found" });

            return Ok(new ApiResponse<CategoryDto> { Success = true, Data = category });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse>> DeleteCategory(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (!result)
                return NotFound(new ApiResponse { Success = false, Message = "Category not found" });

            return Ok(new ApiResponse { Success = true, Message = "Category deleted successfully" });
        }
    }
}
