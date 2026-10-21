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
/// Quản lý các mẫu thiết kế phối sẵn sticker và ốp lưng mẫu cho khách hàng lựa chọn.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("DesignTemplates")]
public class DesignTemplatesController : ControllerBase
{
    private readonly IDesignTemplateService _designTemplateService;

    public DesignTemplatesController(IDesignTemplateService designTemplateService)
    {
        _designTemplateService = designTemplateService;
    }

    /// <summary>
    /// Lấy danh sách tất cả mẫu thiết kế (Admin)
    /// </summary>
    /// <remarks>
    /// Truy xuất danh sách toàn bộ mẫu thiết kế sticker phối sẵn trong hệ thống.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <response code="200">Lấy danh sách mẫu thiết kế thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    [HttpGet]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Lấy danh sách tất cả mẫu thiết kế")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DesignTemplateResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var templates = await _designTemplateService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<DesignTemplateResponse>>.SuccessResponse(templates));
    }

    /// <summary>
    /// Lấy thông tin chi tiết một mẫu thiết kế (Admin)
    /// </summary>
    /// <remarks>
    /// Tra cứu chi tiết mẫu thiết kế sticker theo ID.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của mẫu thiết kế.</param>
    /// <response code="200">Tìm thấy mẫu thiết kế, trả về thông tin chi tiết.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy mẫu thiết kế với ID tương ứng.</response>
    [HttpGet("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Lấy chi tiết mẫu thiết kế theo ID")]
    [ProducesResponseType(typeof(ApiResponse<DesignTemplateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var template = await _designTemplateService.GetByIdAsync(id);
        if (template == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy mẫu thiết kế với ID {id}."));
        }

        return Ok(ApiResponse<DesignTemplateResponse>.SuccessResponse(template));
    }

    /// <summary>
    /// Tạo mới một mẫu thiết kế sticker (Kiosk/Designer)
    /// </summary>
    /// <remarks>
    /// Khởi tạo một mẫu thiết kế sticker mới liên kết với ốp lưng và nhân viên thiết kế.
    /// </remarks>
    /// <param name="request">Thông tin mẫu thiết kế cần tạo.</param>
    /// <response code="201">Tạo mẫu thiết kế thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ.</response>
    [HttpPost]
    [AllowAnonymous]
    [EndpointSummary("Tạo mới một mẫu thiết kế")]
    [ProducesResponseType(typeof(ApiResponse<DesignTemplateResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] DesignTemplateCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (data, errorMessage) = await _designTemplateService.CreateAsync(request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }

        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, ApiResponse<DesignTemplateResponse>.SuccessResponse(data, "Tạo mẫu thiết kế thành công."));
    }

    /// <summary>
    /// Cập nhật thông tin mẫu thiết kế (Admin)
    /// </summary>
    /// <remarks>
    /// Chỉnh sửa tên mẫu, ảnh xem trước hoặc trạng thái kích hoạt của mẫu thiết kế.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của mẫu thiết kế cần cập nhật.</param>
    /// <param name="request">Thông tin cập nhật của mẫu thiết kế.</param>
    /// <response code="200">Cập nhật mẫu thiết kế thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy mẫu thiết kế với ID tương ứng.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Cập nhật thông tin mẫu thiết kế")]
    [ProducesResponseType(typeof(ApiResponse<DesignTemplateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] DesignTemplateUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (data, errorMessage) = await _designTemplateService.UpdateAsync(id, request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }
        if (data == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy mẫu thiết kế với ID {id}."));
        }

        return Ok(ApiResponse<DesignTemplateResponse>.SuccessResponse(data, "Cập nhật mẫu thiết kế thành công."));
    }

    /// <summary>
    /// Xóa một mẫu thiết kế (Admin)
    /// </summary>
    /// <remarks>
    /// Xóa mẫu thiết kế khỏi hệ thống.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh của mẫu thiết kế cần xóa.</param>
    /// <response code="200">Xóa mẫu thiết kế thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy mẫu thiết kế với ID tương ứng.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Xóa một mẫu thiết kế")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _designTemplateService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy mẫu thiết kế với ID {id}."));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Xóa mẫu thiết kế thành công."));
    }
}
