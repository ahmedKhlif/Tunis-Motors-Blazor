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
    public class CarListingsController : ControllerBase
    {
        private readonly ICarListingService _carListingService;
        private readonly ILogger<CarListingsController> _logger;
        private readonly IWebHostEnvironment _hostEnvironment;

        public CarListingsController(ICarListingService carListingService, ILogger<CarListingsController> logger, IWebHostEnvironment hostEnvironment)
        {
            _carListingService = carListingService;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<PaginatedDto<CarListingDto>>>> GetAllListings([FromQuery] CarListingFilterDto filter)
        {
            var result = await _carListingService.GetAllCarListingsAsync(filter);
            return Ok(new ApiResponse<PaginatedDto<CarListingDto>> { Success = true, Data = result });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<CarListingDto>>> GetListing(int id)
        {
            var listing = await _carListingService.GetCarListingByIdAsync(id);
            if (listing == null)
                return NotFound(new ApiResponse { Success = false, Message = "Listing not found" });

            return Ok(new ApiResponse<CarListingDto> { Success = true, Data = listing });
        }

        [HttpPost("{id}/increment-view")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse>> IncrementViews(int id)
        {
            var result = await _carListingService.IncrementViewsAsync(id);
            if (!result)
                return NotFound(new ApiResponse { Success = false, Message = "Listing not found" });

            return Ok(new ApiResponse { Success = true, Message = "View count incremented" });
        }

        [HttpGet("category/{categoryId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<CarListingDto>>>> GetListingsByCategory(int categoryId)
        {
            var listings = await _carListingService.GetCarListingsByCategoryAsync(categoryId);
            return Ok(new ApiResponse<List<CarListingDto>> { Success = true, Data = listings });
        }

        [HttpGet("seller/{sellerId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<CarListingDto>>>> GetSellerListings(string sellerId)
        {
            var listings = await _carListingService.GetUserListingsAsync(sellerId);
            return Ok(new ApiResponse<List<CarListingDto>> { Success = true, Data = listings });
        }

        [HttpPost]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<ActionResult<ApiResponse<CarListingDto>>> CreateListing([FromBody] CreateCarListingDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid input" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var listing = await _carListingService.CreateCarListingAsync(userId, model);

            if (listing == null)
                return BadRequest(new ApiResponse { Success = false, Message = "Failed to create listing" });

            return CreatedAtAction(nameof(GetListing), new { id = listing.ProductId }, 
                new ApiResponse<CarListingDto> { Success = true, Data = listing });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<ActionResult<ApiResponse<CarListingDto>>> UpdateListing(int id, [FromBody] UpdateCarListingDto model)
        {
            var listing = await _carListingService.GetCarListingByIdAsync(id);
            if (listing == null)
                return NotFound(new ApiResponse { Success = false, Message = "Listing not found" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (listing.SellerId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var updatedListing = await _carListingService.UpdateCarListingAsync(id, model);
            return Ok(new ApiResponse<CarListingDto> { Success = true, Data = updatedListing });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<ActionResult<ApiResponse>> DeleteListing(int id)
        {
            var listing = await _carListingService.GetCarListingByIdAsync(id);
            if (listing == null)
                return NotFound(new ApiResponse { Success = false, Message = "Listing not found" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (listing.SellerId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var result = await _carListingService.DeleteCarListingAsync(id);
            if (!result)
                return BadRequest(new ApiResponse { Success = false, Message = "Failed to delete listing" });

            return Ok(new ApiResponse { Success = true, Message = "Listing deleted successfully" });
        }

        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse>> ApproveListing(int id, [FromBody] ApproveListingDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _carListingService.ApproveCarListingAsync(id, userId, model.AdminNote);

            if (!result)
                return BadRequest(new ApiResponse { Success = false, Message = "Failed to approve listing" });

            return Ok(new ApiResponse { Success = true, Message = "Listing approved successfully" });
        }

        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse>> RejectListing(int id, [FromBody] RejectListingDto model)
        {
            var result = await _carListingService.RejectCarListingAsync(id, model.RejectionReason);

            if (!result)
                return BadRequest(new ApiResponse { Success = false, Message = "Failed to reject listing" });

            return Ok(new ApiResponse { Success = true, Message = "Listing rejected successfully" });
        }

        [HttpGet("pending-approvals")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<List<CarListingDto>>>> GetPendingApprovals()
        {
            var listings = await _carListingService.GetPendingApprovalsAsync();
            return Ok(new ApiResponse<List<CarListingDto>> { Success = true, Data = listings });
        }

        [HttpGet("filters/brands")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetBrands()
        {
            var brands = await _carListingService.GetBrandsAsync();
            return Ok(new ApiResponse<List<string>> { Success = true, Data = brands });
        }

        [HttpGet("filters/fuel-types")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetFuelTypes()
        {
            var types = await _carListingService.GetFuelTypesAsync();
            return Ok(new ApiResponse<List<string>> { Success = true, Data = types });
        }

        [HttpGet("filters/transmissions")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetTransmissions()
        {
            var transmissions = await _carListingService.GetTransmissionsAsync();
            return Ok(new ApiResponse<List<string>> { Success = true, Data = transmissions });
        }

        [HttpGet("filters/colors")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetColors()
        {
            var colors = await _carListingService.GetColorsAsync();
            return Ok(new ApiResponse<List<string>> { Success = true, Data = colors });
        }

        [HttpPost("upload-images")]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<ActionResult<ApiResponse<List<string>>>> UploadImages([FromForm] IFormFileCollection files)
        {
            _logger.LogInformation($"[UploadImages] Received upload request. Files count: {files?.Count ?? 0}");
            
            if (files == null || files.Count == 0)
            {
                _logger.LogWarning("[UploadImages] No files received in request");
                return BadRequest(new ApiResponse { Success = false, Message = "No files uploaded" });
            }

            // Get or create wwwroot path
            var webRootPath = _hostEnvironment.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath))
            {
                // Fallback: use ContentRootPath/wwwroot if WebRootPath is not set
                webRootPath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot");
            }

            var uploadsFolder = Path.Combine(webRootPath, "images");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uploadedPaths = new List<string>();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            const long maxFileSize = 10 * 1024 * 1024; // 10MB per file

            foreach (var file in files)
            {
                _logger.LogInformation($"[UploadImages] Processing file: {file.FileName}, Size: {file.Length} bytes, ContentType: {file.ContentType}");
                
                if (file.Length == 0)
                {
                    _logger.LogWarning($"[UploadImages] File {file.FileName} is empty, skipping");
                    continue;
                }

                // Validate file size
                if (file.Length > maxFileSize)
                {
                    _logger.LogWarning($"[UploadImages] File {file.FileName} exceeds max size of 10MB (Size: {file.Length} bytes)");
                    continue;
                }

                // Validate file extension
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
                {
                    _logger.LogWarning($"[UploadImages] File {file.FileName} has invalid extension: {ext}");
                    continue;
                }

                try
                {
                    var fileName = Guid.NewGuid().ToString() + ext;
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    _logger.LogInformation($"[UploadImages] Saving file to: {filePath}");

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    // Store relative path for serving from wwwroot
                    var relativePath = $"/images/{fileName}";
                    uploadedPaths.Add(relativePath);
                    _logger.LogInformation($"[UploadImages] Successfully uploaded: {fileName} -> {relativePath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[UploadImages] Error uploading file {file.FileName}: {ex.Message}");
                    _logger.LogError($"[UploadImages] Stack trace: {ex.StackTrace}");
                    continue;
                }
            }

            if (uploadedPaths.Count == 0)
                return BadRequest(new ApiResponse { Success = false, Message = "No files were successfully uploaded" });

            return Ok(new ApiResponse<List<string>> { Success = true, Data = uploadedPaths, Message = $"{uploadedPaths.Count} file(s) uploaded successfully" });
        }
    }
}
