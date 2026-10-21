using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StickerRobot.Constants;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Utilities;

namespace StickerRobot.Controllers;

/// <summary>
/// Tải lên tệp mô hình 3D (GLB) và hình ảnh sticker (PNG, WebP).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = AuthRoles.CatalogAdmins)]
[Tags("Uploads")]
public class UploadsController : ControllerBase
{
    private const long ModelLimit = 25 * 1024 * 1024;
    private const long ImageLimit = 8 * 1024 * 1024;
    private static readonly byte[] GlbMagic = "glTF"u8.ToArray();
    private static readonly byte[] PngMagic = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

    private readonly IWebHostEnvironment _env;

    public UploadsController(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// Tải lên tệp mô hình 3D (.glb) hoặc ảnh sticker (.png, .webp)
    /// </summary>
    /// <remarks>
    /// Tiếp nhận tệp nhị phân thông qua giao thức multipart/form-data và lưu vào thư mục tĩnh wwwroot/uploads/{kind}/.
    /// - **phone**: Tệp mô hình 3D điện thoại dạng GLB (tối đa 25MB).
    /// - **case**: Tệp mô hình 3D ốp lưng dạng GLB (tối đa 25MB).
    /// - **sticker**: Tệp ảnh sticker dạng PNG hoặc WebP trong suốt (tối đa 8MB).
    /// Yêu cầu quyền: Quản trị viên (Manager hoặc SuperAdmin).
    /// </remarks>
    /// <param name="request">Thông tin tệp nhị phân và phân loại tài nguyên.</param>
    /// <response code="201">Tải lên tệp thành công, trả về đường dẫn URL tĩnh.</response>
    /// <response code="400">Tệp không hợp lệ (sai định dạng, quá giới hạn dung lượng hoặc sai phân loại kind).</response>
    /// <response code="401">Chưa xác thực hoặc token không hợp lệ.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(ModelLimit + 1024 * 1024)]
    [EndpointSummary("Tải lên tệp mô hình 3D hoặc hình ảnh sticker")]
    [ProducesResponseType(typeof(ApiResponse<UploadResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Upload([FromForm] FileUploadRequest request)
    {
        var file = request.File;
        var kind = request.Kind;

        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Tệp tin tải lên không được để trống."));
        }

        if (kind is not ("phone" or "case" or "sticker"))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Tham số kind bắt buộc phải là 'phone', 'case', hoặc 'sticker'."));
        }

        var head = new byte[12];
        await using (var peek = file.OpenReadStream())
        {
            _ = await peek.ReadAsync(head);
        }

        string extension;
        if (kind == "sticker")
        {
            if (file.Length > ImageLimit) return BadRequest(ApiResponse<object>.ErrorResponse("Ảnh sticker tối đa 8 MB."));
            var isPng = head.AsSpan(0, 8).SequenceEqual(PngMagic);
            var isWebp = head.AsSpan(0, 4).SequenceEqual("RIFF"u8) && head.AsSpan(8, 4).SequenceEqual("WEBP"u8);
            if (!isPng && !isWebp) return BadRequest(ApiResponse<object>.ErrorResponse("Ảnh sticker phải là PNG hoặc WebP."));
            extension = isPng ? ".png" : ".webp";
        }
        else
        {
            if (file.Length > ModelLimit) return BadRequest(ApiResponse<object>.ErrorResponse("File model tối đa 25 MB."));
            if (!head.AsSpan(0, 4).SequenceEqual(GlbMagic)) return BadRequest(ApiResponse<object>.ErrorResponse("File model phải là GLB (glTF nhị phân)."));
            extension = ".glb";
        }

        var root = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var folder = Path.Combine(root, "uploads", kind);
        Directory.CreateDirectory(folder);
        var name = $"{Guid.NewGuid():N}{extension}";
        await using (var target = System.IO.File.Create(Path.Combine(folder, name)))
        {
            await file.CopyToAsync(target);
        }

        var url = $"/uploads/{kind}/{name}";
        return Created(url, ApiResponse<UploadResponse>.SuccessResponse(new UploadResponse { Url = url }, "Tải lên tệp thành công."));
    }
}
