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
/// Quản lý danh mục ốp lưng, kích thước thực tế và ma trận vùng dán fit 3D.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("PhoneCases")]
public class PhoneCasesController : ControllerBase
{
    private readonly IPhoneCaseService _phoneCaseService;
    private readonly ICatalogService _catalog;

    public PhoneCasesController(IPhoneCaseService phoneCaseService, ICatalogService catalog)
    {
        _phoneCaseService = phoneCaseService;
        _catalog = catalog;
    }

    /// <summary>
    /// Lấy danh sách tất cả các loại ốp lưng
    /// </summary>
    /// <remarks>
    /// Truy xuất danh sách toàn bộ các ốp lưng điện thoại trong kho (SKU, chất liệu, màu sắc, giá bán, số lượng tồn).
    /// Cho phép truy cập công khai không cần đăng nhập.
    /// </remarks>
    /// <response code="200">Lấy danh sách ốp lưng thành công.</response>
    [HttpGet]
    [AllowAnonymous]
    [EndpointSummary("Lấy danh sách tất cả ốp lưng")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PhoneCaseResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var cases = await _phoneCaseService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<PhoneCaseResponse>>.SuccessResponse(cases));
    }

    /// <summary>
    /// Lấy thông tin chi tiết một ốp lưng
    /// </summary>
    /// <remarks>
    /// Tra cứu chi tiết thông số và giá của một ốp lưng theo ID. Cho phép truy cập công khai.
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của ốp lưng.</param>
    /// <response code="200">Tìm thấy ốp lưng, trả về thông tin chi tiết.</response>
    /// <response code="404">Không tìm thấy ốp lưng với ID đã cung cấp.</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [EndpointSummary("Lấy chi tiết một ốp lưng")]
    [ProducesResponseType(typeof(ApiResponse<PhoneCaseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var phoneCase = await _phoneCaseService.GetByIdAsync(id);
        if (phoneCase == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy ốp lưng với ID {id}."));
        }

        return Ok(ApiResponse<PhoneCaseResponse>.SuccessResponse(phoneCase));
    }

    /// <summary>
    /// Tạo mới một ốp lưng
    /// </summary>
    /// <remarks>
    /// Khởi tạo ốp lưng mới liên kết với một dòng điện thoại trong hệ thống.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="request">Thông tin ốp lưng cần tạo.</param>
    /// <response code="201">Tạo mới ốp lưng thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    [HttpPost]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Tạo mới một ốp lưng")]
    [ProducesResponseType(typeof(ApiResponse<PhoneCaseResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] PhoneCaseCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (data, errorMessage) = await _phoneCaseService.CreateAsync(request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }

        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, ApiResponse<PhoneCaseResponse>.SuccessResponse(data, "Tạo ốp lưng thành công."));
    }

    /// <summary>
    /// Cập nhật thông tin ốp lưng
    /// </summary>
    /// <remarks>
    /// Chỉnh sửa thông tin SKU, chất liệu, màu sắc, đơn giá hoặc số lượng tồn kho của ốp lưng.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của ốp lưng cần cập nhật.</param>
    /// <param name="request">Dữ liệu cập nhật của ốp lưng.</param>
    /// <response code="200">Cập nhật ốp lưng thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy ốp lưng với ID tương ứng.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Cập nhật thông tin ốp lưng")]
    [ProducesResponseType(typeof(ApiResponse<PhoneCaseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] PhoneCaseUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (data, errorMessage) = await _phoneCaseService.UpdateAsync(id, request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }
        if (data == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy ốp lưng với ID {id}."));
        }

        return Ok(ApiResponse<PhoneCaseResponse>.SuccessResponse(data, "Cập nhật ốp lưng thành công."));
    }

    /// <summary>
    /// Xóa một ốp lưng
    /// </summary>
    /// <remarks>
    /// Xóa ốp lưng khỏi kho hệ thống. Nếu ốp lưng đã có đơn hàng dán hoặc mẫu thiết kế tham chiếu, hệ thống sẽ từ chối xóa để đảm bảo toàn vẹn dữ liệu.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh của ốp lưng cần xóa.</param>
    /// <response code="200">Xóa ốp lưng thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy ốp lưng với ID tương ứng.</response>
    /// <response code="409">Không thể xóa vì ốp lưng đang có dữ liệu đơn hàng hoặc mẫu thiết kế liên kết ràng buộc (Conflict).</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Xóa một ốp lưng")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var (found, blocked) = await _phoneCaseService.DeleteAsync(id);
        if (!found)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy ốp lưng với ID {id}."));
        }

        if (blocked != null)
        {
            return Conflict(ApiResponse<object>.ErrorResponse(blocked));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Xóa ốp lưng thành công."));
    }

    /// <summary>
    /// Cấu hình mô hình 3D (GLB), kích thước và ma trận vùng dán fit cho ốp
    /// </summary>
    /// <remarks>
    /// Lưu tệp GLB mô hình ốp lưng, kích thước vật lý mm (chiều cao, bề rộng), ma trận bitmap vùng dán (MaskBase64) và độ thụt 3D (PhoneInsetMm, PhoneScale). Dữ liệu này dùng để tính toán không gian dán nhãn cho robot và dựng 3D trên màn hình Kiosk.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">ID của ốp lưng cần thiết lập thông số fit 3D.</param>
    /// <param name="request">Dữ liệu mô hình 3D, kích thước và ma trận mặt nạ vùng dán.</param>
    /// <response code="200">Lưu thông số fit 3D cho ốp lưng thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ hoặc ma trận mặt nạ không đúng định dạng.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy ốp lưng.</response>
    [HttpPut("{id}/fit")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Cấu hình mô hình 3D và ma trận vùng dán cho ốp")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetFit(int id, [FromBody] PhoneCaseFitRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (found, error) = await _catalog.SetCaseFitAsync(id, request);
        if (!found) return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy ốp lưng {id}."));
        if (error != null) return BadRequest(ApiResponse<object>.ErrorResponse(error));
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Lưu cấu hình 3D và vùng dán thành công."));
    }
}
