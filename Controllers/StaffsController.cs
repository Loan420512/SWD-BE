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
/// Quản lý danh sách nhân viên kỹ thuật vận hành, thiết kế và hỗ trợ kiosk.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = AuthRoles.CatalogAdmins)]
[Tags("Staffs")]
public class StaffsController : ControllerBase
{
    private readonly IStaffService _staffService;

    public StaffsController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    /// <summary>
    /// Lấy danh sách tất cả nhân viên (Admin)
    /// </summary>
    /// <remarks>
    /// Truy xuất danh sách toàn bộ nhân viên vận hành, hỗ trợ và thiết kế kiosk.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <response code="200">Lấy danh sách nhân viên thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    [HttpGet]
    [EndpointSummary("Lấy danh sách tất cả nhân viên")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StaffResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var staffList = await _staffService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<StaffResponse>>.SuccessResponse(staffList));
    }

    /// <summary>
    /// Lấy chi tiết nhân viên theo ID (Admin)
    /// </summary>
    /// <remarks>
    /// Tra cứu thông tin chi tiết một nhân viên gồm họ tên, mã nhân viên, số điện thoại, vị trí và trạng thái hoạt động.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của nhân viên.</param>
    /// <response code="200">Tìm thấy thông tin nhân viên thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy nhân viên với ID tương ứng.</response>
    [HttpGet("{id}")]
    [EndpointSummary("Lấy chi tiết nhân viên theo ID")]
    [ProducesResponseType(typeof(ApiResponse<StaffResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var staff = await _staffService.GetByIdAsync(id);
        if (staff == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy nhân viên với ID {id}."));
        }

        return Ok(ApiResponse<StaffResponse>.SuccessResponse(staff));
    }

    /// <summary>
    /// Tạo mới một nhân viên (Admin)
    /// </summary>
    /// <remarks>
    /// Thêm nhân viên mới vào hệ thống phân quyền của kiosk.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="request">Thông tin nhân viên cần tạo.</param>
    /// <response code="201">Tạo mới nhân viên thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    [HttpPost]
    [EndpointSummary("Tạo mới một nhân viên")]
    [ProducesResponseType(typeof(ApiResponse<StaffResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] StaffCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (data, errorMessage) = await _staffService.CreateAsync(request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }

        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, ApiResponse<StaffResponse>.SuccessResponse(data, "Tạo nhân viên thành công."));
    }

    /// <summary>
    /// Cập nhật thông tin nhân viên (Admin)
    /// </summary>
    /// <remarks>
    /// Chỉnh sửa họ tên, mã nhân viên, số điện thoại, vị trí hoặc trạng thái làm việc.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của nhân viên cần cập nhật.</param>
    /// <param name="request">Thông tin cập nhật của nhân viên.</param>
    /// <response code="200">Cập nhật thông tin nhân viên thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy nhân viên với ID tương ứng.</response>
    [HttpPut("{id}")]
    [EndpointSummary("Cập nhật thông tin nhân viên")]
    [ProducesResponseType(typeof(ApiResponse<StaffResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] StaffUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (data, errorMessage) = await _staffService.UpdateAsync(id, request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }
        if (data == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy nhân viên với ID {id}."));
        }

        return Ok(ApiResponse<StaffResponse>.SuccessResponse(data, "Cập nhật nhân viên thành công."));
    }

    /// <summary>
    /// Xóa một nhân viên (Admin)
    /// </summary>
    /// <remarks>
    /// Xóa nhân viên khỏi hệ thống nếu không có dữ liệu ràng buộc.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh của nhân viên cần xóa.</param>
    /// <response code="200">Xóa nhân viên thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy nhân viên với ID tương ứng.</response>
    [HttpDelete("{id}")]
    [EndpointSummary("Xóa một nhân viên")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _staffService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy nhân viên với ID {id}."));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Xóa nhân viên thành công."));
    }
}
