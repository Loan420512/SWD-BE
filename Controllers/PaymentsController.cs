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
/// Quản lý và ghi nhận các giao dịch thanh toán đơn hàng (tiền mặt, VNPay, Momo, ZaloPay).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// Lấy danh sách tất cả giao dịch thanh toán (Admin)
    /// </summary>
    /// <remarks>
    /// Truy xuất toàn bộ lịch sử thanh toán qua các kênh tiền mặt, VNPay, Momo, ZaloPay.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <response code="200">Lấy danh sách thanh toán thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    [HttpGet]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Lấy danh sách tất cả giao dịch thanh toán")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaymentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var payments = await _paymentService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<PaymentResponse>>.SuccessResponse(payments));
    }

    /// <summary>
    /// Lấy chi tiết một giao dịch thanh toán (Admin)
    /// </summary>
    /// <remarks>
    /// Tra cứu chi tiết giao dịch thanh toán theo ID gồm số tiền, phương thức, mã đối tác và thời điểm thanh toán.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của giao dịch thanh toán.</param>
    /// <response code="200">Tìm thấy giao dịch thanh toán thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy giao dịch thanh toán với ID tương ứng.</response>
    [HttpGet("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Lấy chi tiết giao dịch thanh toán theo ID")]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var payment = await _paymentService.GetByIdAsync(id);
        if (payment == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy giao dịch thanh toán với ID {id}."));
        }

        return Ok(ApiResponse<PaymentResponse>.SuccessResponse(payment));
    }

    /// <summary>
    /// Khởi tạo giao dịch thanh toán (Kiosk)
    /// </summary>
    /// <remarks>
    /// Tạo bản ghi thanh toán cho đơn hàng tại kiosk khi khách hàng chọn phương thức thanh toán tiền mặt hoặc quét mã QR.
    /// Cho phép truy cập công khai tại kiosk.
    /// </remarks>
    /// <param name="request">Thông tin giao dịch thanh toán cần tạo.</param>
    /// <response code="201">Khởi tạo giao dịch thanh toán thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ hoặc đơn hàng không tồn tại.</response>
    [HttpPost]
    [AllowAnonymous]
    [EndpointSummary("Khởi tạo giao dịch thanh toán")]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] PaymentCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (data, errorMessage) = await _paymentService.CreateAsync(request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }

        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, ApiResponse<PaymentResponse>.SuccessResponse(data, "Tạo giao dịch thanh toán thành công."));
    }

    /// <summary>
    /// Cập nhật thông tin giao dịch thanh toán (Admin)
    /// </summary>
    /// <remarks>
    /// Cập nhật trạng thái thanh toán hoặc mã giao dịch đối soát của bên thứ ba.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của giao dịch thanh toán.</param>
    /// <param name="request">Thông tin cập nhật của thanh toán.</param>
    /// <response code="200">Cập nhật thanh toán thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy giao dịch thanh toán với ID tương ứng.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Cập nhật thông tin thanh toán")]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] PaymentUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (data, errorMessage) = await _paymentService.UpdateAsync(id, request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }
        if (data == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy giao dịch thanh toán với ID {id}."));
        }

        return Ok(ApiResponse<PaymentResponse>.SuccessResponse(data, "Cập nhật thanh toán thành công."));
    }

    /// <summary>
    /// Xóa một giao dịch thanh toán (Admin)
    /// </summary>
    /// <remarks>
    /// Xóa bản ghi thanh toán nếu phát sinh sai lệch hoặc hủy giao dịch.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh của giao dịch thanh toán cần xóa.</param>
    /// <response code="200">Xóa thanh toán thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy giao dịch thanh toán với ID tương ứng.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Xóa một giao dịch thanh toán")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _paymentService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy giao dịch thanh toán với ID {id}."));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Xóa giao dịch thanh toán thành công."));
    }
}
