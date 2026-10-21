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
/// Quản lý tài khoản quản trị viên hệ thống cấp cao (Manager, SuperAdmin).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = AuthRoles.CatalogAdmins)]
[Tags("Admins")]
public class AdminsController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Lấy danh sách tất cả quản trị viên (Admin)
    /// </summary>
    /// <remarks>
    /// Truy xuất danh sách toàn bộ các tài khoản quản trị (Manager, SuperAdmin, Admin) trong hệ thống.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <response code="200">Lấy danh sách quản trị viên thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    [HttpGet]
    [EndpointSummary("Lấy danh sách tất cả quản trị viên")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AdminResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var admins = await _adminService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<AdminResponse>>.SuccessResponse(admins));
    }

    /// <summary>
    /// Lấy chi tiết tài khoản quản trị viên theo ID (Admin)
    /// </summary>
    /// <remarks>
    /// Tra cứu chi tiết tài khoản quản trị viên (username, email, role). Mật khẩu băm được bảo mật và không trả về.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của quản trị viên.</param>
    /// <response code="200">Tìm thấy tài khoản quản trị viên thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy quản trị viên với ID tương ứng.</response>
    [HttpGet("{id}")]
    [EndpointSummary("Lấy chi tiết quản trị viên theo ID")]
    [ProducesResponseType(typeof(ApiResponse<AdminResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var admin = await _adminService.GetByIdAsync(id);
        if (admin == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy quản trị viên với ID {id}."));
        }

        return Ok(ApiResponse<AdminResponse>.SuccessResponse(admin));
    }

    /// <summary>
    /// Tạo mới một tài khoản quản trị viên (SuperAdmin)
    /// </summary>
    /// <remarks>
    /// Khởi tạo tài khoản quản trị viên mới. Mật khẩu sẽ được tự động băm bằng thuật toán BCrypt trước khi lưu.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="request">Thông tin tài khoản quản trị viên cần tạo.</param>
    /// <response code="201">Tạo tài khoản quản trị viên thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ hoặc username đã tồn tại.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    [HttpPost]
    [EndpointSummary("Tạo mới tài khoản quản trị viên")]
    [ProducesResponseType(typeof(ApiResponse<AdminResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] AdminCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (data, errorMessage) = await _adminService.CreateAsync(request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }

        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, ApiResponse<AdminResponse>.SuccessResponse(data, "Tạo tài khoản quản trị viên thành công."));
    }

    /// <summary>
    /// Cập nhật thông tin tài khoản quản trị viên (Admin)
    /// </summary>
    /// <remarks>
    /// Thay đổi tên đăng nhập, email, vai trò hoặc đổi mật khẩu mới cho quản trị viên.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của quản trị viên cần cập nhật.</param>
    /// <param name="request">Thông tin cập nhật của quản trị viên.</param>
    /// <response code="200">Cập nhật tài khoản quản trị viên thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy quản trị viên với ID tương ứng.</response>
    [HttpPut("{id}")]
    [EndpointSummary("Cập nhật thông tin quản trị viên")]
    [ProducesResponseType(typeof(ApiResponse<AdminResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] AdminUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (data, errorMessage) = await _adminService.UpdateAsync(id, request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }
        if (data == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy quản trị viên với ID {id}."));
        }

        return Ok(ApiResponse<AdminResponse>.SuccessResponse(data, "Cập nhật tài khoản quản trị viên thành công."));
    }

    /// <summary>
    /// Xóa một tài khoản quản trị viên (SuperAdmin)
    /// </summary>
    /// <remarks>
    /// Xóa tài khoản quản trị viên khỏi hệ thống.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh của tài khoản quản trị viên cần xóa.</param>
    /// <response code="200">Xóa tài khoản quản trị viên thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy tài khoản quản trị viên với ID tương ứng.</response>
    [HttpDelete("{id}")]
    [EndpointSummary("Xóa một tài khoản quản trị viên")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _adminService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy quản trị viên với ID {id}."));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Xóa quản trị viên thành công."));
    }
}
