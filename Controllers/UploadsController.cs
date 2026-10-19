using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StickerRobot.Constants;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Utilities;

namespace StickerRobot.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = AuthRoles.CatalogAdmins)]
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

    /// <summary>Stores a phone GLB, case GLB, or sticker PNG/WebP and returns its site path.</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(ModelLimit + 1024 * 1024)]
    [ProducesResponseType(typeof(ApiResponse<UploadResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(IFormFile file, [FromForm] string kind)
    {
        if (kind is not ("phone" or "case" or "sticker"))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("kind must be phone, case, or sticker."));
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
        return Created(url, ApiResponse<UploadResponse>.SuccessResponse(new UploadResponse { Url = url }, "Uploaded."));
    }
}
