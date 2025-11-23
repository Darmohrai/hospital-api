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

    // ✅ ОНОВИ КОНСТРУКТОР
    public AuthController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        IUpgradeRequestRepository requestRepo) // <-- Новий
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _requestRepo = requestRepo; // <-- Новий
    }

    /// <summary>
    /// 4) "Гість": Реєстрація та відправка заявки
    /// (Цей метод створює користувача одразу з роллю 'Guest')
    /// </summary>
    [HttpPost("register-guest")]
    public async Task<IActionResult> RegisterGuest([FromBody] RegisterDto dto)
    {
        var userExists = await _userManager.FindByNameAsync(dto.Username);
        if (userExists != null)
        {
            // ✅ ВИПРАВЛЕНО: Повертаємо масив JSON, щоб фронтенд розпізнав це як помилку поля "Username"
            return BadRequest(new[] { 
                new { code = "DuplicateUserName", description = "Користувач з таким іменем вже існує." } 
            });
        }
        
        var userExistsEmail = await _userManager.FindByEmailAsync(dto.Email);
        if (userExistsEmail != null)
        {
            // ✅ ВИПРАВЛЕНО: Повертаємо масив JSON, щоб фронтенд розпізнав це як помилку поля "Username"
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

        // Автоматично призначаємо роль "Guest"
        await _userManager.AddToRoleAsync(user, "Guest");

        // ✅ СТВОРЕННЯ ЗАЯВКИ
        var existingRequest = await _requestRepo.GetPendingRequestByUserIdAsync(user.Id);
        if (existingRequest == null) // Створюємо тільки якщо немає активної заявки
        {
            var request = new UpgradeRequest { UserId = user.Id, RequestedRole = dto.Role};
            await _requestRepo.AddAsync(request);
        }

        return Ok(new
            { Status = "Success", Message = "Користувача 'Гість' створено. Заявку надіслано адміністратору." });
    }

    /// <summary>
    /// Вхід у систему для всіх користувачів
    /// </summary>
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

    /// <summary>
    /// 1) "Адміністратор": Додавання операторів
    /// (Також можна використовувати для створення "Авторизованих")
    /// </summary>
    [Authorize(Roles = "Admin")] // Тільки Admin може це робити
    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] RegisterDto dto)
    {
        // ✅ ОНОВЛЕНО: Додано "Admin" у валідацію
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

        // Роль береться з DTO
        await _userManager.AddToRoleAsync(user, dto.Role);

        return Ok(new { Status = "Success", Message = $"Користувача створено з роллю '{dto.Role}'." });
    }
    
    public class ApproveGuestRequest
    {
        public string TargetRole { get; set; }
    }


    /// <summary>
    /// 1) "Адміністратор": Затвердження заявки "Гостя"
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("approve-guest/{userId}")]
    public async Task<IActionResult>
        ApproveGuest(string userId, [FromBody] ApproveGuestRequest approveGuestRequest) // <-- ✅ Додано параметр targetRole
    {

        string targetRole = approveGuestRequest.TargetRole;
        // ✅ Валідація цільової ролі
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

        // ✅ Змінюємо роль на вказану targetRole
        await _userManager.RemoveFromRoleAsync(user, "Guest");
        await _userManager.AddToRoleAsync(user, targetRole);

        // Оновлюємо статус заявки
        request.Status = RequestStatus.Approved;
        await _requestRepo.UpdateAsync(request);

        return Ok(new
            { Status = "Success", Message = $"Користувача підвищено до '{targetRole}', заявку затверджено." });
    }

    /// <summary>
    /// 1) "Адміністратор": Відхилення заявки "Гостя"
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("reject-guest/{userId}")]
    public async Task<IActionResult> RejectGuest(string userId, [FromQuery] string comment = "Причина не вказана")
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound("Користувача не знайдено.");

        var request = await _requestRepo.GetPendingRequestByUserIdAsync(userId);
        if (request == null) return BadRequest("Активної заявки для цього користувача не знайдено.");

        // ✅ ОНОВЛЮЄМО СТАТУС ЗАЯВКИ
        request.Status = RequestStatus.Rejected;
        await _requestRepo.UpdateAsync(request);

        return Ok(new { Status = "Success", Message = $"Заявку відхилено. Причина: {comment}" });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("pending-requests")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var requests = await _requestRepo.GetPendingRequestsAsync();
        // Перетворюємо в DTO, щоб не показувати повний IdentityUser
        var result = requests.Select(r => new
        {
            RequestId = r.Id,
            UserId = r.UserId,
            UserName = r.User?.UserName ?? "N/A", // З репозиторію має прийти User
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
            expires: DateTime.Now.AddHours(3), // Тривалість життя токена
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
        ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto) // Використовуємо RegisterDto, бо там є Email
    {
        var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
        if (user == null)
        {
            // У "справжньому" додатку ми б не казали, що юзер не знайдений,
            // але для курсового це ОК і спрощує відладку.
            return NotFound(new { message = "Користувач з таким email не знайдений." });
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Кодуємо токен для безпечної передачі в URL
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var tokenEncoded = WebEncoders.Base64UrlEncode(tokenBytes);

        // ПОВЕРТАЄМО ТОКЕН НА ФРОНТЕНД
        return Ok(new
        {
            token = tokenEncoded,
            email = user.Email
        });
    }

    // === DTO для скидання (створіть цей клас) ===
    public class ResetPasswordDto
    {
        [Required] public string Email { get; set; }
        [Required] public string Token { get; set; }
        [Required] public string NewPassword { get; set; }
    }


    // === НОВИЙ ЕНДПОІНТ 2: ВСТАНОВЛЕННЯ НОВОГО ПАРОЛЯ ===
    // (Цей метод залишається без змін)
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return BadRequest(new { message = "Помилка: Невірний запит." });
        }

        // Розкодувати токен з URL
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

        // Повернути помилки валідації (напр., "пароль занадто короткий")
        return BadRequest(result.Errors);
    }
}