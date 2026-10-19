using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StickerRobot.Constants;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Services.Interfaces;
using StickerRobot.Utilities;

namespace StickerRobot.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CatalogController : ControllerBase
{
    private readonly ICatalogService _catalog;

    public CatalogController(ICatalogService catalog)
    {
        _catalog = catalog;
    }

    /// <summary>Customer read model: phone models with cases and fits, and active stickers.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CatalogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        return Ok(ApiResponse<CatalogResponse>.SuccessResponse(await _catalog.GetCatalogAsync()));
    }
}

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = AuthRoles.CatalogAdmins)]
public class StickerProductsController : ControllerBase
{
    private readonly ICatalogService _catalog;

    public StickerProductsController(ICatalogService catalog)
    {
        _catalog = catalog;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StickerProductResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(ApiResponse<IEnumerable<StickerProductResponse>>.SuccessResponse(await _catalog.GetStickersAsync(includeInactive: true)));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StickerProductResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] StickerProductRequest request)
    {
        var (data, error) = await _catalog.CreateStickerAsync(request);
        if (error != null) return BadRequest(ApiResponse<object>.ErrorResponse(error));
        return Created($"/api/StickerProducts/{data!.Id}", ApiResponse<StickerProductResponse>.SuccessResponse(data, "Sticker created."));
    }

    /// <summary>Edits a sticker. Retire it with isActive false; there is no delete.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StickerProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] StickerProductRequest request)
    {
        var (data, error) = await _catalog.UpdateStickerAsync(id, request);
        if (error != null) return BadRequest(ApiResponse<object>.ErrorResponse(error));
        if (data == null) return NotFound(ApiResponse<object>.ErrorResponse($"Sticker {id} not found."));
        return Ok(ApiResponse<StickerProductResponse>.SuccessResponse(data, "Sticker updated."));
    }
}
