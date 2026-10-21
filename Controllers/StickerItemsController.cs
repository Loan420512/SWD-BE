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
/// Quản lý chi tiết vị trí tọa độ, kích thước và góc xoay của sticker trên từng mẫu thiết kế.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("StickerItems")]
public class StickerItemsController : ControllerBase
{
    private readonly IStickerItemService _stickerItemService;

    public StickerItemsController(IStickerItemService stickerItemService)
    {
        _stickerItemService = stickerItemService;
    }

    /// <summary>
    /// Lấy danh sách tất cả chi tiết phần tử sticker (Admin)
    /// </summary>
    /// <remarks>
    /// Truy xuất danh sách toàn bộ các vị trí tọa độ sticker được gắn vào mẫu thiết kế.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <response code="200">Lấy danh sách sticker item thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    [HttpGet]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Lấy danh sách tất cả sticker item")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StickerItemResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var items = await _stickerItemService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<StickerItemResponse>>.SuccessResponse(items));
    }

    /// <summary>
    /// Lấy chi tiết một phần tử sticker theo ID (Admin)
    /// </summary>
    /// <remarks>
    /// Tra cứu chi tiết vị trí tọa độ X, Y, góc xoay và tỷ lệ phóng to/thu nhỏ của một sticker theo ID.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của sticker item.</param>
    /// <response code="200">Tìm thấy sticker item thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy sticker item với ID tương ứng.</response>
    [HttpGet("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Lấy chi tiết sticker item theo ID")]
    [ProducesResponseType(typeof(ApiResponse<StickerItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _stickerItemService.GetByIdAsync(id);
        if (item == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy sticker item với ID {id}."));
        }

        return Ok(ApiResponse<StickerItemResponse>.SuccessResponse(item));
    }

    /// <summary>
    /// Gắn một phần tử sticker vào mẫu thiết kế (Kiosk/Designer)
    /// </summary>
    /// <remarks>
    /// Thêm một phần tử sticker với tọa độ (PositionX, PositionY), góc xoay (RotationDeg) vào mẫu thiết kế.
    /// </remarks>
    /// <param name="request">Thông tin phần tử sticker cần gán.</param>
    /// <response code="201">Gắn sticker vào mẫu thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ hoặc mẫu thiết kế không tồn tại.</response>
    [HttpPost]
    [AllowAnonymous]
    [EndpointSummary("Gắn một sticker vào mẫu thiết kế")]
    [ProducesResponseType(typeof(ApiResponse<StickerItemResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] StickerItemCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (data, errorMessage) = await _stickerItemService.CreateAsync(request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }

        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, ApiResponse<StickerItemResponse>.SuccessResponse(data, "Gắn sticker vào mẫu thành công."));
    }

    /// <summary>
    /// Cập nhật tọa độ hoặc thông số của phần tử sticker (Admin)
    /// </summary>
    /// <remarks>
    /// Điều chỉnh vị trí tọa độ X, Y, tỷ lệ kích thước hoặc góc xoay của sticker trên mẫu.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của sticker item.</param>
    /// <param name="request">Thông tin cập nhật của sticker item.</param>
    /// <response code="200">Cập nhật vị trí sticker thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy sticker item với ID tương ứng.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Cập nhật tọa độ sticker trên mẫu")]
    [ProducesResponseType(typeof(ApiResponse<StickerItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] StickerItemUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (data, errorMessage) = await _stickerItemService.UpdateAsync(id, request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }
        if (data == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy sticker item với ID {id}."));
        }

        return Ok(ApiResponse<StickerItemResponse>.SuccessResponse(data, "Cập nhật vị trí sticker thành công."));
    }

    /// <summary>
    /// Gỡ bỏ một sticker khỏi mẫu thiết kế (Admin)
    /// </summary>
    /// <remarks>
    /// Xóa phần tử sticker khỏi mẫu thiết kế.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh của sticker item cần xóa.</param>
    /// <response code="200">Xóa sticker item thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy sticker item với ID tương ứng.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Gỡ bỏ một sticker khỏi mẫu")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _stickerItemService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy sticker item với ID {id}."));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Xóa sticker item thành công."));
    }
}
