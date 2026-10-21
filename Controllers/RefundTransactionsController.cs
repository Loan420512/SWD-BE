using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StickerRobot.Constants;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Services.Interfaces;
using StickerRobot.Utilities;

namespace StickerRobot.Controllers;

/// <summary>
/// Quản lý giao dịch và lịch sử hoàn tiền cho khách hàng.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = AuthRoles.CatalogAdmins)]
[Tags("RefundTransactions")]
public class RefundTransactionsController : ControllerBase
{
    private readonly IRefundTransactionService _refundTransactionService;

    public RefundTransactionsController(IRefundTransactionService refundTransactionService)
    {
        _refundTransactionService = refundTransactionService;
    }

    /// <summary>
    /// Lấy danh sách tất cả giao dịch hoàn tiền (Admin)
    /// </summary>
    /// <remarks>
    /// Truy xuất danh sách toàn bộ các yêu cầu và lịch sử hoàn tiền cho khách hàng.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <response code="200">Lấy danh sách hoàn tiền thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    [HttpGet]
    [EndpointSummary("Lấy danh sách tất cả giao dịch hoàn tiền")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RefundTransactionResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var refunds = await _refundTransactionService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<RefundTransactionResponse>>.SuccessResponse(refunds));
    }

    /// <summary>
    /// Lấy chi tiết giao dịch hoàn tiền theo ID (Admin)
    /// </summary>
    /// <remarks>
    /// Tra cứu chi tiết một giao dịch hoàn tiền theo ID gồm số tiền hoàn, lý do, người duyệt và trạng thái phê duyệt.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của giao dịch hoàn tiền.</param>
    /// <response code="200">Tìm thấy giao dịch hoàn tiền thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy giao dịch hoàn tiền với ID tương ứng.</response>
    [HttpGet("{id}")]
    [EndpointSummary("Lấy chi tiết giao dịch hoàn tiền theo ID")]
    [ProducesResponseType(typeof(ApiResponse<RefundTransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var refund = await _refundTransactionService.GetByIdAsync(id);
        if (refund == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy giao dịch hoàn tiền với ID {id}."));
        }

        return Ok(ApiResponse<RefundTransactionResponse>.SuccessResponse(refund));
    }

    /// <summary>
    /// Tạo mới một yêu cầu hoàn tiền (Admin)
    /// </summary>
    /// <remarks>
    /// Khởi tạo yêu cầu hoàn tiền cho một giao dịch thanh toán khi phát sinh sự cố in dán hỏng hoặc lỗi thiết bị.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="request">Thông tin yêu cầu hoàn tiền cần tạo.</param>
    /// <response code="201">Tạo yêu cầu hoàn tiền thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ hoặc giao dịch thanh toán không tồn tại.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    [HttpPost]
    [EndpointSummary("Tạo mới yêu cầu hoàn tiền")]
    [ProducesResponseType(typeof(ApiResponse<RefundTransactionResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] RefundTransactionCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (data, errorMessage) = await _refundTransactionService.CreateAsync(request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }

        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, ApiResponse<RefundTransactionResponse>.SuccessResponse(data, "Tạo yêu cầu hoàn tiền thành công."));
    }

    /// <summary>
    /// Cập nhật thông tin hoặc trạng thái hoàn tiền (Admin)
    /// </summary>
    /// <remarks>
    /// Cập nhật trạng thái duyệt hoàn tiền sang 'Approved' hoặc 'Rejected'.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của giao dịch hoàn tiền.</param>
    /// <param name="request">Thông tin cập nhật của giao dịch hoàn tiền.</param>
    /// <response code="200">Cập nhật giao dịch hoàn tiền thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy giao dịch hoàn tiền với ID tương ứng.</response>
    [HttpPut("{id}")]
    [EndpointSummary("Cập nhật giao dịch hoàn tiền")]
    [ProducesResponseType(typeof(ApiResponse<RefundTransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] RefundTransactionUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (data, errorMessage) = await _refundTransactionService.UpdateAsync(id, request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }
        if (data == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy giao dịch hoàn tiền với ID {id}."));
        }

        return Ok(ApiResponse<RefundTransactionResponse>.SuccessResponse(data, "Cập nhật giao dịch hoàn tiền thành công."));
    }

    /// <summary>
    /// Xóa một giao dịch hoàn tiền (Admin)
    /// </summary>
    /// <remarks>
    /// Xóa bản ghi hoàn tiền nếu tạo nhầm lẫn.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh của giao dịch hoàn tiền cần xóa.</param>
    /// <response code="200">Xóa giao dịch hoàn tiền thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy giao dịch hoàn tiền với ID tương ứng.</response>
    [HttpDelete("{id}")]
    [EndpointSummary("Xóa một giao dịch hoàn tiền")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _refundTransactionService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy giao dịch hoàn tiền với ID {id}."));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Xóa giao dịch hoàn tiền thành công."));
    }
}
