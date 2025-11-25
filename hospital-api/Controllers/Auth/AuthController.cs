using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using hospital_api.DTOs.Auth;
using hospital_api.Models.Auth;
using hospital_api.Repositories.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;

namespace hospital_api.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    private readonly IUpgradeRequestRepository _requestRepo;
    
    public AuthController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        IUpgradeRequestRepository requestRepo)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _requestRepo = requestRepo;
    }

    [HttpPost("register-guest")]
    public async Task<IActionResult> RegisterGuest([FromBody] RegisterDto dto)
    {
        var userExists = await _userManager.FindByNameAsync(dto.Username);
        if (userExists != null)
        {
            return BadRequest(new[] { 
                new { code = "DuplicateUserName", description = "Користувач з таким іменем вже існує." } 
            });
        }
        
        var userExistsEmail = await _userManager.FindByEmailAsync(dto.Email);
        if (userExistsEmail != null)
        {
            return BadRequest(new[] { 
                new { code = "DuplicateUserName", description = "Користувач з таким email вже існує." } 
            });
        }

        var user = new IdentityUser
        {
            Email = dto.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = dto.Username
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _userManager.AddToRoleAsync(user, "Guest");

        var existingRequest = await _requestRepo.GetPendingRequestByUserIdAsync(user.Id);
        if (existingRequest == null)
        {
            var request = new UpgradeRequest { UserId = user.Id, RequestedRole = dto.Role};
            await _requestRepo.AddAsync(request);
        }

        return Ok(new
            { Status = "Success", Message = "Користувача 'Гість' створено. Заявку надіслано адміністратору." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.Username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized("Неправильний логін або пароль.");

        var userRoles = await _userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

        foreach (var userRole in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, userRole));
        }

        var token = GenerateJwtToken(authClaims);

        return Ok(new AuthResponseDto
        {
            UserId = user.Id,
            Username = user.UserName,
            Email = user.Email,
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Roles = userRoles.ToList()
        });
    }

    [Authorize(Roles = "Admin")] // Тільки Admin може це робити
    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] RegisterDto dto)
    {
        if (dto.Role != "Operator" && dto.Role != "Authorized" && dto.Role != "Admin")
            return BadRequest("Можна створювати тільки 'Operator', 'Authorized' або 'Admin'.");

        var userExists = await _userManager.FindByNameAsync(dto.Username);
        if (userExists != null)
            return BadRequest("Користувач з таким іменем вже існує.");

        var user = new IdentityUser
        {
            Email = dto.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = dto.Username
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _userManager.AddToRoleAsync(user, dto.Role);

        return Ok(new { Status = "Success", Message = $"Користувача створено з роллю '{dto.Role}'." });
    }
    
    public class ApproveGuestRequest
    {
        public string TargetRole { get; set; }
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPost("approve-guest/{userId}")]
    public async Task<IActionResult>
        ApproveGuest(string userId, [FromBody] ApproveGuestRequest approveGuestRequest)
    {

        string targetRole = approveGuestRequest.TargetRole;
        if (targetRole != "Authorized" && targetRole != "Operator" && targetRole != "Admin")
        {
            return BadRequest("Цільова роль має бути 'Authorized' або 'Operator'.");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound("Користувача не знайдено.");

        var request = await _requestRepo.GetPendingRequestByUserIdAsync(userId);
        if (request == null) return BadRequest("Активної заявки для цього користувача не знайдено.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Guest")) return BadRequest("Користувач не є 'Гостем'.");

        await _userManager.RemoveFromRoleAsync(user, "Guest");
        await _userManager.AddToRoleAsync(user, targetRole);

        request.Status = RequestStatus.Approved;
        await _requestRepo.UpdateAsync(request);

        return Ok(new
            { Status = "Success", Message = $"Користувача підвищено до '{targetRole}', заявку затверджено." });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("reject-guest/{userId}")]
    public async Task<IActionResult> RejectGuest(string userId, [FromQuery] string comment = "Причина не вказана")
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound("Користувача не знайдено.");

        var request = await _requestRepo.GetPendingRequestByUserIdAsync(userId);
        if (request == null) return BadRequest("Активної заявки для цього користувача не знайдено.");

        request.Status = RequestStatus.Rejected;
        await _requestRepo.UpdateAsync(request);

        return Ok(new { Status = "Success", Message = $"Заявку відхилено. Причина: {comment}" });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("pending-requests")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var requests = await _requestRepo.GetPendingRequestsAsync();
        var result = requests.Select(r => new
        {
            RequestId = r.Id,
            UserId = r.UserId,
            UserName = r.User?.UserName ?? "N/A",
            RequestDate = r.RequestDate,
            Role = r.RequestedRole
        });
        return Ok(result);
    }


    private JwtSecurityToken GenerateJwtToken(List<Claim> authClaims)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            expires: DateTime.Now.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }

    public class ForgotPasswordDto
    {
        public string Email { get; set; }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult>
        ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
        if (user == null)
        {
            return NotFound(new { message = "Користувач з таким email не знайдений." });
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var tokenEncoded = WebEncoders.Base64UrlEncode(tokenBytes);

        return Ok(new
        {
            token = tokenEncoded,
            email = user.Email
        });
    }

    public class ResetPasswordDto
    {
        [Required] public string Email { get; set; }
        [Required] public string Token { get; set; }
        [Required] public string NewPassword { get; set; }
    }
    
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return BadRequest(new { message = "Помилка: Невірний запит." });
        }

        byte[] tokenDecodedBytes;
        try
        {
            tokenDecodedBytes = WebEncoders.Base64UrlDecode(model.Token);
        }
        catch (Exception)
        {
            return BadRequest(new { message = "Помилка: Невірний токен." });
        }

        var tokenDecoded = Encoding.UTF8.GetString(tokenDecodedBytes);

        var result = await _userManager.ResetPasswordAsync(user, tokenDecoded, model.NewPassword);

        if (result.Succeeded)
        {
            return Ok(new { message = "Пароль успішно скинуто." });
        }

        return BadRequest(result.Errors);
    }
}