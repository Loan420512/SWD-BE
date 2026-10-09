using Microsoft.AspNetCore.Mvc;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Services.Interfaces;
using StickerRobot.Utilities;

namespace StickerRobot.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AdminsController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AdminResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var admins = await _adminService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<AdminResponse>>.SuccessResponse(admins));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<AdminResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var admin = await _adminService.GetByIdAsync(id);
        if (admin == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Admin with ID {id} not found."));
        }

        return Ok(ApiResponse<AdminResponse>.SuccessResponse(admin));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AdminResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] AdminCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid payload.", ModelState));
        }

        var (data, errorMessage) = await _adminService.CreateAsync(request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }

        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, ApiResponse<AdminResponse>.SuccessResponse(data, "Admin created successfully."));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<AdminResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] AdminUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid payload.", ModelState));
        }

        var (data, errorMessage) = await _adminService.UpdateAsync(id, request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }
        if (data == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Admin with ID {id} not found."));
        }

        return Ok(ApiResponse<AdminResponse>.SuccessResponse(data, "Admin updated successfully."));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _adminService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Admin with ID {id} not found."));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Admin deleted successfully."));
    }
}
