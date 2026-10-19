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
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AdminOrderResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _orderService.GetAdminListAsync();
        return Ok(ApiResponse<IEnumerable<AdminOrderResponse>>.SuccessResponse(orders));
    }

    /// <summary>Public lookup by order code. No customer contact data.</summary>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(ApiResponse<PublicOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code)
    {
        var order = await _orderService.GetByCodeAsync(code.Trim().ToUpperInvariant());
        if (order == null) return NotFound(ApiResponse<object>.ErrorResponse($"Order {code} not found."));
        return Ok(ApiResponse<PublicOrderResponse>.SuccessResponse(order));
    }

    /// <summary>Pending to Processing or Cancelled; Processing to Completed or Cancelled.</summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetStatus(int id, [FromBody] OrderStatusRequest request)
    {
        var (found, error) = await _orderService.SetStatusAsync(id, request.Status);
        if (!found) return NotFound(ApiResponse<object>.ErrorResponse($"Order {id} not found."));
        if (error != null) return BadRequest(ApiResponse<object>.ErrorResponse(error));
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Status updated."));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Order with ID {id} not found."));
        }

        return Ok(ApiResponse<OrderResponse>.SuccessResponse(order));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] OrderCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid payload.", ModelState));
        }

        var (data, errorMessage) = await _orderService.CreateAsync(request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }

        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, ApiResponse<OrderResponse>.SuccessResponse(data, "Order created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] OrderUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid payload.", ModelState));
        }

        var (data, errorMessage) = await _orderService.UpdateAsync(id, request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }
        if (data == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Order with ID {id} not found."));
        }

        return Ok(ApiResponse<OrderResponse>.SuccessResponse(data, "Order updated successfully."));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _orderService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Order with ID {id} not found."));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Order deleted successfully."));
    }
}
