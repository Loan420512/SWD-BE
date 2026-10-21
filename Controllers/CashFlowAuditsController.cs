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
/// Đối soát và kiểm toán dòng tiền mặt tại kiosk so với hệ thống thanh toán.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = AuthRoles.CatalogAdmins)]
[Tags("CashFlowAudits")]
public class CashFlowAuditsController : ControllerBase
{
    private readonly ICashFlowAuditService _cashFlowAuditService;

    public CashFlowAuditsController(ICashFlowAuditService cashFlowAuditService)
    {
        _cashFlowAuditService = cashFlowAuditService;
    }

    /// <summary>
    /// Lấy danh sách tất cả kỳ kiểm toán đối soát (Admin)
    /// </summary>
    /// <remarks>
    /// Truy xuất lịch sử các kỳ đối soát doanh thu tiền mặt và hoàn tiền tại kiosk.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <response code="200">Lấy danh sách kiểm toán thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    [HttpGet]
    [EndpointSummary("Lấy danh sách tất cả kỳ kiểm toán")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CashFlowAuditResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var audits = await _cashFlowAuditService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<CashFlowAuditResponse>>.SuccessResponse(audits));
    }

    /// <summary>
    /// Lấy chi tiết kỳ kiểm toán theo ID (Admin)
    /// </summary>
    /// <remarks>
    /// Tra cứu chi tiết một phiên đối soát dòng tiền theo ID gồm ngày đối soát, tổng thu, tổng hoàn và trạng thái đối soát.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của bản ghi kiểm toán.</param>
    /// <response code="200">Tìm thấy kỳ kiểm toán thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy bản ghi kiểm toán với ID tương ứng.</response>
    [HttpGet("{id}")]
    [EndpointSummary("Lấy chi tiết kỳ kiểm toán theo ID")]
    [ProducesResponseType(typeof(ApiResponse<CashFlowAuditResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var audit = await _cashFlowAuditService.GetByIdAsync(id);
        if (audit == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy bản ghi kiểm toán với ID {id}."));
        }

        return Ok(ApiResponse<CashFlowAuditResponse>.SuccessResponse(audit));
    }

    /// <summary>
    /// Tạo mới một kỳ kiểm toán dòng tiền (Admin)
    /// </summary>
    /// <remarks>
    /// Khởi tạo phiên đối soát doanh thu và tiền hoàn trả tại kiosk.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="request">Thông tin phiên đối soát cần tạo.</param>
    /// <response code="201">Tạo phiên kiểm toán thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    [HttpPost]
    [EndpointSummary("Tạo mới một kỳ kiểm toán")]
    [ProducesResponseType(typeof(ApiResponse<CashFlowAuditResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CashFlowAuditCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (data, errorMessage) = await _cashFlowAuditService.CreateAsync(request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }

        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, ApiResponse<CashFlowAuditResponse>.SuccessResponse(data, "Tạo bản ghi kiểm toán thành công."));
    }

    /// <summary>
    /// Cập nhật thông tin kỳ kiểm toán (Admin)
    /// </summary>
    /// <remarks>
    /// Cập nhật số tiền doanh thu, tiền hoàn trả hoặc trạng thái đối soát sau khi kiểm kê thực tế.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của bản ghi kiểm toán.</param>
    /// <param name="request">Thông tin cập nhật của phiên kiểm toán.</param>
    /// <response code="200">Cập nhật bản ghi kiểm toán thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy bản ghi kiểm toán với ID tương ứng.</response>
    [HttpPut("{id}")]
    [EndpointSummary("Cập nhật kỳ kiểm toán")]
    [ProducesResponseType(typeof(ApiResponse<CashFlowAuditResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CashFlowAuditUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (data, errorMessage) = await _cashFlowAuditService.UpdateAsync(id, request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }
        if (data == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy bản ghi kiểm toán với ID {id}."));
        }

        return Ok(ApiResponse<CashFlowAuditResponse>.SuccessResponse(data, "Cập nhật bản ghi kiểm toán thành công."));
    }

    /// <summary>
    /// Xóa một kỳ kiểm toán (Admin)
    /// </summary>
    /// <remarks>
    /// Xóa bản ghi đối soát nếu tạo sai lệch.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh của bản ghi kiểm toán cần xóa.</param>
    /// <response code="200">Xóa bản ghi kiểm toán thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy bản ghi kiểm toán với ID tương ứng.</response>
    [HttpDelete("{id}")]
    [EndpointSummary("Xóa một kỳ kiểm toán")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _cashFlowAuditService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy bản ghi kiểm toán với ID {id}."));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Xóa bản ghi kiểm toán thành công."));
    }
}
