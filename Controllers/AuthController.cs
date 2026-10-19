using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StickerRobot.Constants;
using StickerRobot.Data;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Utilities;

namespace StickerRobot.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly StickerRobotDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(StickerRobotDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var admin = await _context.Admins.AsNoTracking().FirstOrDefaultAsync(a => a.Username == request.Username);
        var valid = admin?.PasswordHash != null && BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash);
        if (!valid || admin!.Role is not (UserRoles.Manager or UserRoles.SuperAdmin))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse("Sai tên đăng nhập hoặc mật khẩu."));
        }

        var expires = DateTime.UtcNow.AddHours(12);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
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
        }));
    }
}
