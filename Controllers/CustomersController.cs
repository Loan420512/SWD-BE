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
/// Quản lý thông tin và lịch sử khách hàng tại kiosk.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Lấy danh sách tất cả khách hàng (Admin)
    /// </summary>
    /// <remarks>
    /// Truy xuất danh sách hồ sơ toàn bộ khách hàng đã từng sử dụng dịch vụ tại hệ thống kiosk.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <response code="200">Lấy danh sách khách hàng thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    [HttpGet]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Lấy danh sách tất cả khách hàng")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CustomerResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var customers = await _customerService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<CustomerResponse>>.SuccessResponse(customers));
    }

    /// <summary>
    /// Lấy thông tin chi tiết một khách hàng (Admin)
    /// </summary>
    /// <remarks>
    /// Tra cứu hồ sơ chi tiết của khách hàng theo ID.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của khách hàng.</param>
    /// <response code="200">Tìm thấy khách hàng, trả về thông tin chi tiết.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy khách hàng với ID tương ứng.</response>
    [HttpGet("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Lấy chi tiết khách hàng theo ID")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy khách hàng với ID {id}."));
        }

        return Ok(ApiResponse<CustomerResponse>.SuccessResponse(customer));
    }

    /// <summary>
    /// Tạo mới thông tin khách hàng (Kiosk)
    /// </summary>
    /// <remarks>
    /// Ghi nhận thông tin khách hàng (họ tên, số điện thoại nhận thông báo, email) tại kiosk trong quá trình thanh toán/đặt in dán.
    /// Cho phép truy cập công khai không yêu cầu token Admin.
    /// </remarks>
    /// <param name="request">Thông tin khách hàng cần tạo mới.</param>
    /// <response code="201">Tạo mới hồ sơ khách hàng thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ (sai định dạng email hoặc số điện thoại).</response>
    [HttpPost]
    [AllowAnonymous]
    [EndpointSummary("Tạo mới thông tin khách hàng")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CustomerCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var customer = await _customerService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, ApiResponse<CustomerResponse>.SuccessResponse(customer, "Tạo khách hàng thành công."));
    }

    /// <summary>
    /// Cập nhật thông tin khách hàng (Admin)
    /// </summary>
    /// <remarks>
    /// Chỉnh sửa họ tên, số điện thoại hoặc email liên hệ của khách hàng.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh duy nhất của khách hàng cần cập nhật.</param>
    /// <param name="request">Thông tin cập nhật của khách hàng.</param>
    /// <response code="200">Cập nhật thông tin khách hàng thành công.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy khách hàng với ID tương ứng.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Cập nhật thông tin khách hàng")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CustomerUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var customer = await _customerService.UpdateAsync(id, request);
        if (customer == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy khách hàng với ID {id}."));
        }

        return Ok(ApiResponse<CustomerResponse>.SuccessResponse(customer, "Cập nhật khách hàng thành công."));
    }

    /// <summary>
    /// Xóa thông tin khách hàng (Admin)
    /// </summary>
    /// <remarks>
    /// Xóa hồ sơ khách hàng nếu khách hàng chưa có đơn hàng liên kết.
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="id">Mã định danh của khách hàng cần xóa.</param>
    /// <response code="200">Xóa khách hàng thành công.</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    /// <response code="404">Không tìm thấy khách hàng với ID tương ứng.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = AuthRoles.CatalogAdmins)]
    [EndpointSummary("Xóa thông tin khách hàng")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _customerService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy khách hàng với ID {id}."));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Xóa khách hàng thành công."));
    }
}
