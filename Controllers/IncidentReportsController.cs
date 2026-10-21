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
/// Ghi nhận và theo dõi xử lý sự cố kỹ thuật của kiosk robot.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = AuthRoles.CatalogAdmins)]
[Tags("IncidentReports")]
public class IncidentReportsController : ControllerBase
{
    private readonly IIncidentReportService _incidentReportService;

    public IncidentReportsController(IIncidentReportService incidentReportService)
    {
        _incidentReportService = incidentReportService;
    }

    /// <summary>
    /// Lấy danh sách tất cả báo cáo sự cố (Admin)
    /// </summary>
    /// <remarks>
    /// Truy xuất danh sách toàn bộ sự cố kỹ thuật phát sinh tại kiosk (kẹt khay in, hỏng đầu in, lỗi cảm biến,...).
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <response code="200">Lấy danh sách báo cáo sự cố thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    [HttpGet]
    [EndpointSummary("Lấy danh sách tất cả báo cáo sự cố")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<IncidentReportResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var reports = await _incidentReportService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<IncidentReportResponse>>.SuccessResponse(reports));
    }

    /// <summary>
    /// Lấy chi tiết báo cáo sự cố theo ID (Admin)
    /// </summary>
    /// <remarks>
    /// Tra cứu chi tiết một sự cố gồm người báo cáo, phân loại, mô tả hiện tượng và trạng thái xử lý.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của báo cáo sự cố.</param>
    /// <response code="200">Tìm thấy báo cáo sự cố thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy báo cáo sự cố với ID tương ứng.</response>
    [HttpGet("{id}")]
    [EndpointSummary("Lấy chi tiết báo cáo sự cố theo ID")]
    [ProducesResponseType(typeof(ApiResponse<IncidentReportResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var report = await _incidentReportService.GetByIdAsync(id);
        if (report == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy báo cáo sự cố với ID {id}."));
        }

        return Ok(ApiResponse<IncidentReportResponse>.SuccessResponse(report));
    }

    /// <summary>
    /// Tạo mới báo cáo sự cố kỹ thuật (Admin)
    /// </summary>
    /// <remarks>
    /// Ghi nhận một sự cố mới phát sinh tại kiosk để thông báo cho đội ngũ kỹ thuật.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="request">Thông tin sự cố cần báo cáo.</param>
    /// <response code="201">Tạo báo cáo sự cố thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    [HttpPost]
    [EndpointSummary("Tạo mới báo cáo sự cố")]
    [ProducesResponseType(typeof(ApiResponse<IncidentReportResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] IncidentReportCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (data, errorMessage) = await _incidentReportService.CreateAsync(request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }

        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, ApiResponse<IncidentReportResponse>.SuccessResponse(data, "Tạo báo cáo sự cố thành công."));
    }

    /// <summary>
    /// Cập nhật tiến trình hoặc trạng thái sự cố (Admin)
    /// </summary>
    /// <remarks>
    /// Cập nhật phương án xử lý hoặc chuyển trạng thái sự cố sang 'In Progress' hoặc 'Resolved'.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của báo cáo sự cố.</param>
    /// <param name="request">Thông tin cập nhật của báo cáo sự cố.</param>
    /// <response code="200">Cập nhật báo cáo sự cố thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy báo cáo sự cố với ID tương ứng.</response>
    [HttpPut("{id}")]
    [EndpointSummary("Cập nhật báo cáo sự cố")]
    [ProducesResponseType(typeof(ApiResponse<IncidentReportResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] IncidentReportUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (data, errorMessage) = await _incidentReportService.UpdateAsync(id, request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }
        if (data == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy báo cáo sự cố với ID {id}."));
        }

        return Ok(ApiResponse<IncidentReportResponse>.SuccessResponse(data, "Cập nhật báo cáo sự cố thành công."));
    }

    /// <summary>
    /// Xóa một báo cáo sự cố (Admin)
    /// </summary>
    /// <remarks>
    /// Xóa bản ghi sự cố khỏi hệ thống.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh của báo cáo sự cố cần xóa.</param>
    /// <response code="200">Xóa báo cáo sự cố thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy báo cáo sự cố với ID tương ứng.</response>
    [HttpDelete("{id}")]
    [EndpointSummary("Xóa một báo cáo sự cố")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _incidentReportService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy báo cáo sự cố với ID {id}."));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Xóa báo cáo sự cố thành công."));
    }
}
