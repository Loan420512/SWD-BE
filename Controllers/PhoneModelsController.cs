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
/// Quản lý các dòng máy điện thoại và thông số mô hình 3D (GLB).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("PhoneModels")]
public class PhoneModelsController : ControllerBase
{
    private readonly IPhoneModelService _phoneModelService;
    private readonly ICatalogService _catalog;

    public PhoneModelsController(IPhoneModelService phoneModelService, ICatalogService catalog)
    {
        _phoneModelService = phoneModelService;
        _catalog = catalog;
    }

    /// <summary>
    /// Lấy danh sách tất cả các dòng điện thoại
    /// </summary>
    /// <remarks>
    /// Truy xuất danh sách toàn bộ các dòng máy điện thoại (thương hiệu, tên model, kích thước) được hệ thống hỗ trợ.
    /// Cho phép truy cập công khai không cần đăng nhập.
    /// </remarks>
    /// <response code="200">Lấy danh sách dòng điện thoại thành công.</response>
    [HttpGet]
    [AllowAnonymous]
    [EndpointSummary("Lấy danh sách tất cả dòng điện thoại")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PhoneModelResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var models = await _phoneModelService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<PhoneModelResponse>>.SuccessResponse(models));
    }

    /// <summary>
    /// Lấy thông tin chi tiết một dòng điện thoại
    /// </summary>
    /// <remarks>
    /// Tìm kiếm thông tin chi tiết của một dòng máy theo ID định danh. Cho phép truy cập công khai.
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của dòng điện thoại.</param>
    /// <response code="200">Tìm thấy dòng máy, trả về thông tin chi tiết.</response>
    /// <response code="404">Không tìm thấy dòng điện thoại với ID đã cung cấp.</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [EndpointSummary("Lấy chi tiết dòng điện thoại")]
    [ProducesResponseType(typeof(ApiResponse<PhoneModelResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var model = await _phoneModelService.GetByIdAsync(id);
        if (model == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy dòng điện thoại với ID {id}."));
        }

        return Ok(ApiResponse<PhoneModelResponse>.SuccessResponse(model));
    }

    /// <summary>
    /// Tạo mới một dòng điện thoại
    /// </summary>
    /// <remarks>
    /// Khởi tạo một dòng điện thoại mới vào danh mục hệ thống.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="request">Dữ liệu tạo mới dòng máy (Brand, ModelName, Dimensions).</param>
    /// <response code="201">Tạo mới dòng máy thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    [HttpPost]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Tạo mới dòng điện thoại")]
    [ProducesResponseType(typeof(ApiResponse<PhoneModelResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] PhoneModelCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var model = await _phoneModelService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = model.Id }, ApiResponse<PhoneModelResponse>.SuccessResponse(model, "Tạo dòng điện thoại thành công."));
    }

    /// <summary>
    /// Cập nhật thông tin dòng điện thoại
    /// </summary>
    /// <remarks>
    /// Chỉnh sửa thương hiệu, tên model hoặc kích thước vật lý của dòng điện thoại đã có.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của dòng điện thoại cần cập nhật.</param>
    /// <param name="request">Thông tin dòng điện thoại cần cập nhật.</param>
    /// <response code="200">Cập nhật thông tin thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy dòng điện thoại với ID tương ứng.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Cập nhật thông tin dòng điện thoại")]
    [ProducesResponseType(typeof(ApiResponse<PhoneModelResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] PhoneModelUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var model = await _phoneModelService.UpdateAsync(id, request);
        if (model == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy dòng điện thoại với ID {id}."));
        }

        return Ok(ApiResponse<PhoneModelResponse>.SuccessResponse(model, "Cập nhật dòng điện thoại thành công."));
    }

    /// <summary>
    /// Xóa một dòng điện thoại
    /// </summary>
    /// <remarks>
    /// Xóa dòng điện thoại khỏi hệ thống. Thao tác này sẽ bị từ chối nếu dòng máy đang có ốp lưng hoặc dữ liệu liên kết ràng buộc.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh của dòng máy cần xóa.</param>
    /// <response code="200">Xóa dòng điện thoại thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy dòng điện thoại với ID tương ứng.</response>
    /// <response code="409">Không thể xóa vì dòng máy đang có dữ liệu ốp lưng hoặc liên kết ràng buộc (Conflict).</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Xóa một dòng điện thoại")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var (found, blocked) = await _phoneModelService.DeleteAsync(id);
        if (!found)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy dòng điện thoại với ID {id}."));
        }

        if (blocked != null)
        {
            return Conflict(ApiResponse<object>.ErrorResponse(blocked));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Xóa dòng điện thoại thành công."));
    }

    /// <summary>
    /// Cấu hình mô hình 3D (GLB) và hướng trục cho điện thoại
    /// </summary>
    /// <remarks>
    /// Lưu đường dẫn file GLB và thiết lập hệ tọa độ 3D (BackAxis, UpAxis) để máy Kiosk dựng chính xác vị trí thân máy trong không gian 3D.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">ID dòng điện thoại cần cấu hình 3D.</param>
    /// <param name="request">Thông số mô hình 3D và các trục chuẩn.</param>
    /// <response code="200">Cấu hình mô hình 3D thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ hoặc sai định dạng trục.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy dòng điện thoại.</response>
    [HttpPut("{id}/fit")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Cấu hình mô hình 3D (GLB) cho điện thoại")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetFit(int id, [FromBody] PhoneModelFitRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (found, error) = await _catalog.SetModelFitAsync(id, request);
        if (!found) return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy dòng điện thoại {id}."));
        if (error != null) return BadRequest(ApiResponse<object>.ErrorResponse(error));
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Lưu cấu hình 3D thành công."));
    }
}
