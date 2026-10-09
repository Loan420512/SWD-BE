using Microsoft.AspNetCore.Mvc;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Services.Interfaces;
using StickerRobot.Utilities;

namespace StickerRobot.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StickerItemsController : ControllerBase
{
    private readonly IStickerItemService _stickerItemService;

    public StickerItemsController(IStickerItemService stickerItemService)
    {
        _stickerItemService = stickerItemService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StickerItemResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var items = await _stickerItemService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<StickerItemResponse>>.SuccessResponse(items));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StickerItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _stickerItemService.GetByIdAsync(id);
        if (item == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"StickerItem with ID {id} not found."));
        }

        return Ok(ApiResponse<StickerItemResponse>.SuccessResponse(item));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StickerItemResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] StickerItemCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid payload.", ModelState));
        }

        var (data, errorMessage) = await _stickerItemService.CreateAsync(request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }

        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, ApiResponse<StickerItemResponse>.SuccessResponse(data, "StickerItem created successfully."));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StickerItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] StickerItemUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid payload.", ModelState));
        }

        var (data, errorMessage) = await _stickerItemService.UpdateAsync(id, request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }
        if (data == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"StickerItem with ID {id} not found."));
        }

        return Ok(ApiResponse<StickerItemResponse>.SuccessResponse(data, "StickerItem updated successfully."));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _stickerItemService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"StickerItem with ID {id} not found."));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "StickerItem deleted successfully."));
    }
}
