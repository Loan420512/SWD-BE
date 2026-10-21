using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StickerRobot.Constants;
using StickerRobot.Data;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Utilities;

namespace StickerRobot.Controllers;

/// <summary>
/// Xác thực tài khoản quản trị viên và cấp phát mã thông báo JWT.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Auth")]
public class AuthController : ControllerBase
{
    private readonly StickerRobotDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(StickerRobotDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Đăng nhập hệ thống quản trị
    /// </summary>
    /// <remarks>
    /// Tiếp nhận thông tin tài khoản (username và mật khẩu dạng plain text). Mật khẩu được đối soát với chuỗi băm BCrypt trong bảng ADMIN.
    /// Yêu cầu tài khoản phải có vai trò 'Manager' hoặc 'SuperAdmin'.
    /// Khi thành công, hệ thống cấp phát một JWT Bearer token có thời hạn 12 giờ dùng để ủy quyền cho các API quản trị tiếp theo.
    /// </remarks>
    /// <param name="request">Thông tin đăng nhập của quản trị viên.</param>
    /// <response code="200">Đăng nhập thành công, trả về JWT token và vai trò của tài khoản.</response>
    /// <response code="400">Dữ liệu yêu cầu không hợp lệ (thiếu username hoặc password).</response>
    /// <response code="401">Sai tên đăng nhập, mật khẩu không chính xác hoặc tài khoản không có quyền truy cập.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [EndpointSummary("Đăng nhập hệ thống quản trị")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", ModelState));
        }

        var admin = await _context.Admins.AsNoTracking().FirstOrDefaultAsync(a => a.Username == request.Username);
        var valid = admin?.PasswordHash != null && BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash);
        if (!valid || admin!.Role is not (UserRoles.Manager or UserRoles.SuperAdmin))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse("Sai tên đăng nhập hoặc mật khẩu."));
        }

        var expires = DateTime.UtcNow.AddHours(12);
        var jwtKey = _configuration["Jwt:Key"] ?? _configuration["Jwt:SecretKey"] ?? "StickerRobotSuperSecretKeyForJwtAuthentication2026!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "StickerRobot",
            audience: _configuration["Jwt:Audience"] ?? "StickerRobotUsers",
            claims: new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, admin.Id.ToString()),
                new Claim(ClaimTypes.Name, admin.Username!),
                new Claim(ClaimTypes.Role, admin.Role!)
            },
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return Ok(ApiResponse<LoginResponse>.SuccessResponse(new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Username = admin.Username!,
            Role = admin.Role!,
            ExpiresAt = expires
        }, "Đăng nhập thành công."));
    }
}
