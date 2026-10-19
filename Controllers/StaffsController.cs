using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StickerRobot.Constants;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Services.Interfaces;
using StickerRobot.Utilities;

namespace StickerRobot.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = AuthRoles.CatalogAdmins)]
public class StaffsController : ControllerBase
{
    private readonly IStaffService _staffService;

    public StaffsController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StaffResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var staffList = await _staffService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<StaffResponse>>.SuccessResponse(staffList));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StaffResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var staff = await _staffService.GetByIdAsync(id);
        if (staff == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Staff with ID {id} not found."));
        }

        return Ok(ApiResponse<StaffResponse>.SuccessResponse(staff));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StaffResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] StaffCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid payload.", ModelState));
        }

        var (data, errorMessage) = await _staffService.CreateAsync(request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }

        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, ApiResponse<StaffResponse>.SuccessResponse(data, "Staff created successfully."));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StaffResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] StaffUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid payload.", ModelState));
        }

        var (data, errorMessage) = await _staffService.UpdateAsync(id, request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }
        if (data == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Staff with ID {id} not found."));
        }

        return Ok(ApiResponse<StaffResponse>.SuccessResponse(data, "Staff updated successfully."));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _staffService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Staff with ID {id} not found."));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Staff deleted successfully."));
    }
}
