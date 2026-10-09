using Microsoft.AspNetCore.Mvc;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Services.Interfaces;
using StickerRobot.Utilities;

namespace StickerRobot.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DesignTemplatesController : ControllerBase
{
    private readonly IDesignTemplateService _designTemplateService;

    public DesignTemplatesController(IDesignTemplateService designTemplateService)
    {
        _designTemplateService = designTemplateService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DesignTemplateResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var templates = await _designTemplateService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<DesignTemplateResponse>>.SuccessResponse(templates));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DesignTemplateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var template = await _designTemplateService.GetByIdAsync(id);
        if (template == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"DesignTemplate with ID {id} not found."));
        }

        return Ok(ApiResponse<DesignTemplateResponse>.SuccessResponse(template));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DesignTemplateResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] DesignTemplateCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid payload.", ModelState));
        }

        var (data, errorMessage) = await _designTemplateService.CreateAsync(request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }

        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, ApiResponse<DesignTemplateResponse>.SuccessResponse(data, "DesignTemplate created successfully."));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DesignTemplateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] DesignTemplateUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid payload.", ModelState));
        }

        var (data, errorMessage) = await _designTemplateService.UpdateAsync(id, request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }
        if (data == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"DesignTemplate with ID {id} not found."));
        }

        return Ok(ApiResponse<DesignTemplateResponse>.SuccessResponse(data, "DesignTemplate updated successfully."));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _designTemplateService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"DesignTemplate with ID {id} not found."));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "DesignTemplate deleted successfully."));
    }
}
