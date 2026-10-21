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
/// Truy xuất danh mục tổng hợp dành cho Kiosk.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Catalog")]
public class CatalogController : ControllerBase
{
    private readonly ICatalogService _catalog;

    public CatalogController(ICatalogService catalog)
    {
        _catalog = catalog;
    }

    /// <summary>
    /// Lấy dữ liệu danh mục tổng hợp cho Kiosk
    /// </summary>
    /// <remarks>
    /// Trả về mô hình dữ liệu đọc dành cho Kiosk: toàn bộ dòng điện thoại hỗ trợ, danh sách ốp lưng tương thích (kèm ma trận vùng dán fit 3D 1mm) và danh mục các sticker đang được kích hoạt mở bán.
    /// Endpoint này là công khai (không yêu cầu đăng nhập) để ứng dụng Kiosk có thể tải nhanh toàn bộ tài nguyên khi khởi động.
    /// </remarks>
    /// <response code="200">Truy xuất danh mục thành công.</response>
    [HttpGet]
    [AllowAnonymous]
    [EndpointSummary("Lấy dữ liệu danh mục cho Kiosk")]
    [ProducesResponseType(typeof(ApiResponse<CatalogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        return Ok(ApiResponse<CatalogResponse>.SuccessResponse(await _catalog.GetCatalogAsync()));
    }
}

/// <summary>
/// Quản lý thư viện sản phẩm sticker thương mại in sẵn để bán tại Kiosk.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = AuthRoles.CatalogAdmins)]
[Tags("StickerProducts")]
public class StickerProductsController : ControllerBase
{
    private readonly ICatalogService _catalog;

    public StickerProductsController(ICatalogService catalog)
    {
        _catalog = catalog;
    }

    /// <summary>
    /// Lấy danh sách tất cả sản phẩm sticker (Admin)
    /// </summary>
    /// <remarks>
    /// Truy xuất toàn bộ danh mục sticker trong hệ thống, bao gồm cả các mẫu đang bán (IsActive = true) và các mẫu đã tạm ngừng kinh doanh (IsActive = false).
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <response code="200">Lấy danh sách sản phẩm sticker thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    [HttpGet]
    [EndpointSummary("Lấy danh sách tất cả sản phẩm sticker")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StickerProductResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(ApiResponse<IEnumerable<StickerProductResponse>>.SuccessResponse(await _catalog.GetStickersAsync(includeInactive: true)));
    }

    /// <summary>
    /// Tạo mới một sản phẩm sticker
    /// </summary>
    /// <remarks>
    /// Thêm một mẫu sticker in mới vào hệ thống với kích thước vật lý (chiều dài, chiều rộng tính theo milimet) và đơn giá bán lẻ.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="request">Thông tin sản phẩm sticker cần tạo.</param>
    /// <response code="201">Tạo mới sticker thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ hoặc thông số kỹ thuật bị sai lệch.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    [HttpPost]
    [EndpointSummary("Tạo mới sản phẩm sticker")]
    [ProducesResponseType(typeof(ApiResponse<StickerProductResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] StickerProductRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (data, error) = await _catalog.CreateStickerAsync(request);
        if (error != null) return BadRequest(ApiResponse<object>.ErrorResponse(error));
        return Created($"/api/StickerProducts/{data!.Id}", ApiResponse<StickerProductResponse>.SuccessResponse(data, "Tạo sticker thành công."));
    }

    /// <summary>
    /// Cập nhật sản phẩm sticker
    /// </summary>
    /// <remarks>
    /// Chỉnh sửa tên, đường dẫn ảnh, kích thước mm hoặc giá bán của sticker. Để ngừng bán mẫu sticker, hãy cập nhật trường IsActive về false (hệ thống không cung cấp API xóa vật lý để bảo toàn lịch sử đơn hàng đã in).
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của sản phẩm sticker.</param>
    /// <param name="request">Thông tin cập nhật của sticker.</param>
    /// <response code="200">Cập nhật sản phẩm sticker thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy sticker với ID tương ứng.</response>
    [HttpPut("{id}")]
    [EndpointSummary("Cập nhật sản phẩm sticker")]
    [ProducesResponseType(typeof(ApiResponse<StickerProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] StickerProductRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (data, error) = await _catalog.UpdateStickerAsync(id, request);
        if (error != null) return BadRequest(ApiResponse<object>.ErrorResponse(error));
        if (data == null) return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy sticker với ID {id}."));
        return Ok(ApiResponse<StickerProductResponse>.SuccessResponse(data, "Cập nhật sticker thành công."));
    }
}
