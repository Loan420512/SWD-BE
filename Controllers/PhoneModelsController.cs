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
public class PhoneModelsController : ControllerBase
{
    private readonly IPhoneModelService _phoneModelService;
    private readonly ICatalogService _catalog;

    public PhoneModelsController(IPhoneModelService phoneModelService, ICatalogService catalog)
    {
        _phoneModelService = phoneModelService;
        _catalog = catalog;
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
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
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
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
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
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var (found, blocked) = await _phoneModelService.DeleteAsync(id);
        if (!found)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"PhoneModel with ID {id} not found."));
        }

        if (blocked != null)
        {
            return Conflict(ApiResponse<object>.ErrorResponse(blocked));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "PhoneModel deleted successfully."));
    }

    /// <summary>Saves the imported GLB and its orientation for this PhoneModel.</summary>
    [HttpPut("{id}/fit")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetFit(int id, [FromBody] PhoneModelFitRequest request)
    {
        var (found, error) = await _catalog.SetModelFitAsync(id, request);
        if (!found) return NotFound(ApiResponse<object>.ErrorResponse("PhoneModel " + id + " not found."));
        if (error != null) return BadRequest(ApiResponse<object>.ErrorResponse(error));
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Fit saved."));
    }
}
