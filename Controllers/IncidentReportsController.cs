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
public class IncidentReportsController : ControllerBase
{
    private readonly IIncidentReportService _incidentReportService;

    public IncidentReportsController(IIncidentReportService incidentReportService)
    {
        _incidentReportService = incidentReportService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<IncidentReportResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var reports = await _incidentReportService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<IncidentReportResponse>>.SuccessResponse(reports));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<IncidentReportResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var report = await _incidentReportService.GetByIdAsync(id);
        if (report == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"IncidentReport with ID {id} not found."));
        }

        return Ok(ApiResponse<IncidentReportResponse>.SuccessResponse(report));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<IncidentReportResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] IncidentReportCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid payload.", ModelState));
        }

        var (data, errorMessage) = await _incidentReportService.CreateAsync(request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }

        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, ApiResponse<IncidentReportResponse>.SuccessResponse(data, "IncidentReport created successfully."));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<IncidentReportResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] IncidentReportUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid payload.", ModelState));
        }

        var (data, errorMessage) = await _incidentReportService.UpdateAsync(id, request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }
        if (data == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"IncidentReport with ID {id} not found."));
        }

        return Ok(ApiResponse<IncidentReportResponse>.SuccessResponse(data, "IncidentReport updated successfully."));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _incidentReportService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"IncidentReport with ID {id} not found."));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "IncidentReport deleted successfully."));
    }
}
