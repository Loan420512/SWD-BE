using Microsoft.AspNetCore.Mvc;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Services.Interfaces;
using StickerRobot.Utilities;

namespace StickerRobot.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CashFlowAuditsController : ControllerBase
{
    private readonly ICashFlowAuditService _cashFlowAuditService;

    public CashFlowAuditsController(ICashFlowAuditService cashFlowAuditService)
    {
        _cashFlowAuditService = cashFlowAuditService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CashFlowAuditResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var audits = await _cashFlowAuditService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<CashFlowAuditResponse>>.SuccessResponse(audits));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CashFlowAuditResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var audit = await _cashFlowAuditService.GetByIdAsync(id);
        if (audit == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"CashFlowAudit with ID {id} not found."));
        }

        return Ok(ApiResponse<CashFlowAuditResponse>.SuccessResponse(audit));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CashFlowAuditResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CashFlowAuditCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid payload.", ModelState));
        }

        var (data, errorMessage) = await _cashFlowAuditService.CreateAsync(request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }

        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, ApiResponse<CashFlowAuditResponse>.SuccessResponse(data, "CashFlowAudit created successfully."));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CashFlowAuditResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CashFlowAuditUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid payload.", ModelState));
        }

        var (data, errorMessage) = await _cashFlowAuditService.UpdateAsync(id, request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }
        if (data == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"CashFlowAudit with ID {id} not found."));
        }

        return Ok(ApiResponse<CashFlowAuditResponse>.SuccessResponse(data, "CashFlowAudit updated successfully."));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _cashFlowAuditService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"CashFlowAudit with ID {id} not found."));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "CashFlowAudit deleted successfully."));
    }
}
