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
/// Quản lý đơn hàng dán sticker tại kiosk, cập nhật trạng thái đơn và tra cứu đơn theo mã.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Lấy danh sách tất cả đơn hàng (Admin)
    /// </summary>
    /// <remarks>
    /// Truy xuất danh sách toàn bộ các đơn hàng trong hệ thống kèm thông tin khách hàng, mẫu ốp, sticker và trạng thái thanh toán phục vụ việc theo dõi vận hành của robot.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <response code="200">Lấy danh sách đơn hàng thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    [HttpGet]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Lấy danh sách tất cả đơn hàng")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AdminOrderResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _orderService.GetAdminListAsync();
        return Ok(ApiResponse<IEnumerable<AdminOrderResponse>>.SuccessResponse(orders));
    }

    /// <summary>
    /// Tra cứu đơn hàng theo mã đơn công khai
    /// </summary>
    /// <remarks>
    /// Cho phép khách hàng hoặc màn hình chờ của kiosk kiểm tra tiến trình in và dán sticker của robot dựa trên mã đơn hàng (ví dụ: ORD-20261020-0001).
    /// Để bảo mật thông tin riêng tư của khách hàng, endpoint này không trả về số điện thoại hoặc thông tin liên hệ.
    /// Cho phép truy cập công khai không cần token.
    /// </remarks>
    /// <param name="code">Mã đơn hàng công khai (không phân biệt chữ hoa, chữ thường).</param>
    /// <response code="200">Tìm thấy đơn hàng, trả về thông tin tiến trình và tọa độ dán.</response>
    /// <response code="404">Không tìm thấy đơn hàng với mã tương ứng.</response>
    [HttpGet("by-code/{code}")]
    [AllowAnonymous]
    [EndpointSummary("Tra cứu đơn hàng theo mã đơn")]
    [ProducesResponseType(typeof(ApiResponse<PublicOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code)
    {
        var order = await _orderService.GetByCodeAsync(code.Trim().ToUpperInvariant());
        if (order == null) return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy đơn hàng với mã {code}."));
        return Ok(ApiResponse<PublicOrderResponse>.SuccessResponse(order));
    }

    /// <summary>
    /// Cập nhật trạng thái tiến trình đơn hàng
    /// </summary>
    /// <remarks>
    /// Chuyển đổi trạng thái đơn hàng theo luồng xử lý của hệ thống:
    /// - Từ 'Pending' (Chờ in) sang 'Processing' (Đang in/dán) hoặc 'Cancelled' (Hủy đơn).
    /// - Từ 'Processing' (Đang in) sang 'Completed' (Hoàn tất) hoặc 'Cancelled' (Hủy đơn khi gặp sự cố).
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của đơn hàng.</param>
    /// <param name="request">Trạng thái đơn hàng mới cần chuyển sang.</param>
    /// <response code="200">Cập nhật trạng thái đơn hàng thành công.</response>
    /// <response code="400">Trạng thái mới không hợp lệ hoặc bước chuyển trạng thái vi phạm quy trình nghiệp vụ.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy đơn hàng với ID tương ứng.</response>
    [HttpPut("{id}/status")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Cập nhật trạng thái tiến trình đơn hàng")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetStatus(int id, [FromBody] OrderStatusRequest request)
    {
        var (found, error) = await _orderService.SetStatusAsync(id, request.Status);
        if (!found) return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy đơn hàng {id}."));
        if (error != null) return BadRequest(ApiResponse<object>.ErrorResponse(error));
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Cập nhật trạng thái đơn hàng thành công."));
    }

    /// <summary>
    /// Lấy chi tiết đơn hàng theo ID (Admin)
    /// </summary>
    /// <remarks>
    /// Truy xuất thông tin cơ bản của đơn hàng theo ID nội bộ.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của đơn hàng.</param>
    /// <response code="200">Tìm thấy đơn hàng thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy đơn hàng với ID tương ứng.</response>
    [HttpGet("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Lấy chi tiết đơn hàng theo ID")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy đơn hàng với ID {id}."));
        }

        return Ok(ApiResponse<OrderResponse>.SuccessResponse(order));
    }

    /// <summary>
    /// Tạo mới một đơn hàng (Kiosk)
    /// </summary>
    /// <remarks>
    /// Tạo mới một yêu cầu in dán từ màn hình Kiosk sau khi khách hàng hoàn tất thiết kế hoặc chọn mẫu.
    /// Cho phép truy cập công khai tại kiosk mà không yêu cầu đăng nhập token Admin.
    /// </remarks>
    /// <param name="request">Thông tin đơn hàng gồm mã đơn (tùy chọn), khách hàng, ốp lưng, mẫu thiết kế và tổng tiền.</param>
    /// <response code="201">Tạo mới đơn hàng thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ hoặc thông số liên kết không tồn tại.</response>
    [HttpPost]
    [AllowAnonymous]
    [EndpointSummary("Tạo mới một đơn hàng")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] OrderCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (data, errorMessage) = await _orderService.CreateAsync(request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }

        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, ApiResponse<OrderResponse>.SuccessResponse(data, "Tạo đơn hàng thành công."));
    }

    /// <summary>
    /// Cập nhật thông tin đơn hàng (Admin)
    /// </summary>
    /// <remarks>
    /// Chỉnh sửa các trường thông tin đơn hàng như tổng tiền, khách hàng hoặc mẫu thiết kế.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của đơn hàng cần cập nhật.</param>
    /// <param name="request">Dữ liệu cập nhật đơn hàng.</param>
    /// <response code="200">Cập nhật đơn hàng thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy đơn hàng với ID tương ứng.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Cập nhật thông tin đơn hàng")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] OrderUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var (data, errorMessage) = await _orderService.UpdateAsync(id, request);
        if (errorMessage != null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(errorMessage));
        }
        if (data == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy đơn hàng với ID {id}."));
        }

        return Ok(ApiResponse<OrderResponse>.SuccessResponse(data, "Cập nhật đơn hàng thành công."));
    }

    /// <summary>
    /// Xóa một đơn hàng (Admin)
    /// </summary>
    /// <remarks>
    /// Hủy và xóa đơn hàng khỏi hệ thống nếu hợp lệ.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của đơn hàng cần xóa.</param>
    /// <response code="200">Xóa đơn hàng thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy đơn hàng với ID tương ứng.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Xóa một đơn hàng")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _orderService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy đơn hàng với ID {id}."));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Xóa đơn hàng thành công."));
    }
}
