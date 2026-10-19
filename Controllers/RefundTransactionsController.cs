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
public class RefundTransactionsController : ControllerBase
{
    private readonly IRefundTransactionService _refundTransactionService;

    public RefundTransactionsController(IRefundTransactionService refundTransactionService)
    {
        _refundTransactionService = refundTransactionService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RefundTransactionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var refunds = await _refundTransactionService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<RefundTransactionResponse>>.SuccessResponse(refunds));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<RefundTransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var refund = await _refundTransactionService.GetByIdAsync(id);
        if (refund == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"RefundTransaction with ID {id} not found."));
        }

        return Ok(ApiResponse<RefundTransactionResponse>.SuccessResponse(refund));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RefundTransactionResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] RefundTransactionCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid payload.", ModelState));
        }

        var (data, errorMessage) = await _refundTransactionService.CreateAsync(request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }

        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, ApiResponse<RefundTransactionResponse>.SuccessResponse(data, "RefundTransaction created successfully."));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<RefundTransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] RefundTransactionUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid payload.", ModelState));
        }

        var (data, errorMessage) = await _refundTransactionService.UpdateAsync(id, request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }
        if (data == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"RefundTransaction with ID {id} not found."));
        }

        return Ok(ApiResponse<RefundTransactionResponse>.SuccessResponse(data, "RefundTransaction updated successfully."));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _refundTransactionService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"RefundTransaction with ID {id} not found."));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "RefundTransaction deleted successfully."));
    }
}
