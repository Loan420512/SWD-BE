using Microsoft.AspNetCore.Mvc;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Services.Interfaces;
using StickerRobot.Utilities;

namespace StickerRobot.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PhoneModelsController : ControllerBase
{
    private readonly IPhoneModelService _phoneModelService;

    public PhoneModelsController(IPhoneModelService phoneModelService)
    {
        _phoneModelService = phoneModelService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PhoneModelResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var models = await _phoneModelService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<PhoneModelResponse>>.SuccessResponse(models));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<PhoneModelResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var model = await _phoneModelService.GetByIdAsync(id);
        if (model == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"PhoneModel with ID {id} not found."));
        }

        return Ok(ApiResponse<PhoneModelResponse>.SuccessResponse(model));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PhoneModelResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] PhoneModelCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid payload.", ModelState));
        }

        var model = await _phoneModelService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = model.Id }, ApiResponse<PhoneModelResponse>.SuccessResponse(model, "PhoneModel created successfully."));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<PhoneModelResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] PhoneModelUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid payload.", ModelState));
        }

        var model = await _phoneModelService.UpdateAsync(id, request);
        if (model == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"PhoneModel with ID {id} not found."));
        }

        return Ok(ApiResponse<PhoneModelResponse>.SuccessResponse(model, "PhoneModel updated successfully."));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _phoneModelService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"PhoneModel with ID {id} not found."));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "PhoneModel deleted successfully."));
    }
}
