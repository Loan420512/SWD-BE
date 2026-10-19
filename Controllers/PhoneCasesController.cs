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
public class PhoneCasesController : ControllerBase
{
    private readonly IPhoneCaseService _phoneCaseService;
    private readonly ICatalogService _catalog;

    public PhoneCasesController(IPhoneCaseService phoneCaseService, ICatalogService catalog)
    {
        _phoneCaseService = phoneCaseService;
        _catalog = catalog;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PhoneCaseResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var cases = await _phoneCaseService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<PhoneCaseResponse>>.SuccessResponse(cases));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<PhoneCaseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var phoneCase = await _phoneCaseService.GetByIdAsync(id);
        if (phoneCase == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"PhoneCase with ID {id} not found."));
        }

        return Ok(ApiResponse<PhoneCaseResponse>.SuccessResponse(phoneCase));
    }

    [HttpPost]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [ProducesResponseType(typeof(ApiResponse<PhoneCaseResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] PhoneCaseCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid payload.", ModelState));
        }

        var (data, errorMessage) = await _phoneCaseService.CreateAsync(request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }

        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, ApiResponse<PhoneCaseResponse>.SuccessResponse(data, "PhoneCase created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [ProducesResponseType(typeof(ApiResponse<PhoneCaseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] PhoneCaseUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid payload.", ModelState));
        }

        var (data, errorMessage) = await _phoneCaseService.UpdateAsync(id, request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }
        if (data == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"PhoneCase with ID {id} not found."));
        }

        return Ok(ApiResponse<PhoneCaseResponse>.SuccessResponse(data, "PhoneCase updated successfully."));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var (found, blocked) = await _phoneCaseService.DeleteAsync(id);
        if (!found)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"PhoneCase with ID {id} not found."));
        }

        if (blocked != null)
        {
            return Conflict(ApiResponse<object>.ErrorResponse(blocked));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "PhoneCase deleted successfully."));
    }

    /// <summary>Saves the imported GLB and its orientation for this PhoneCase.</summary>
    [HttpPut("{id}/fit")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetFit(int id, [FromBody] PhoneCaseFitRequest request)
    {
        var (found, error) = await _catalog.SetCaseFitAsync(id, request);
        if (!found) return NotFound(ApiResponse<object>.ErrorResponse("PhoneCase " + id + " not found."));
        if (error != null) return BadRequest(ApiResponse<object>.ErrorResponse(error));
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Fit saved."));
    }
}
