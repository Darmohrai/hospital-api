using hospital_api.DTOs.Staff;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers
{
    [Authorize]
    [ApiController]
    [Route("api/staff/gynecologists")] // Більш чіткий маршрут
    public class GynecologistController : ControllerBase
    {
        private readonly IGynecologistService _gynecologistService;

        public GynecologistController(IGynecologistService gynecologistService)
        {
            _gynecologistService = gynecologistService;
        }

        /// <summary>
        /// Отримує список всіх гінекологів.
        /// </summary>
        [Authorize(Roles = "Authorized, Operator, Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var gynecologists = await _gynecologistService.GetAllAsync();
            return Ok(gynecologists);
        }

        /// <summary>
        /// Отримує гінеколога за його ID.
        /// </summary>
        [Authorize(Roles = "Authorized, Operator, Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var gynecologist = await _gynecologistService.GetByIdAsync(id);
            if (gynecologist == null)
                return NotFound();

            return Ok(gynecologist);
        }

        /// <summary>
        /// Створює нового гінеколога.
        /// </summary>
        [Authorize(Roles = "Operator, Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGynecologistDto dto)
        {
            var gynecologist = new Gynecologist
            {
                FullName = dto.FullName,
                WorkExperienceYears = dto.WorkExperienceYears,
                AcademicDegree = dto.AcademicDegree,
                AcademicTitle = dto.AcademicTitle,
            };

            var result = await _gynecologistService.CreateAsync(gynecologist);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.ErrorMessage });

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }

        /// <summary>
        /// Оновлює дані існуючого гінеколога.
        /// </summary>
        [Authorize(Roles = "Operator, Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Gynecologist gynecologist)
        {
            if (id != gynecologist.Id)
                return BadRequest("ID mismatch.");

            var result = await _gynecologistService.UpdateAsync(gynecologist);

            if (!result.IsSuccess)
                return NotFound(new { message = result.ErrorMessage });

            return NoContent();
        }

        /// <summary>
        /// Видаляє гінеколога за його ID.
        /// </summary>
        [Authorize(Roles = "Operator, Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _gynecologistService.DeleteAsync(id);

            if (!result.IsSuccess)
                return NotFound(new { message = result.ErrorMessage });

            return NoContent();
        }

        /// <summary>
        /// Отримує гінекологів з кількістю операцій не менше вказаної.
        /// </summary>
        [Authorize(Roles = "Authorized, Operator, Admin")]
        [HttpGet("min-operations/{count}")]
        public async Task<IActionResult> GetByMinimumOperationCount(int count)
        {
            var result = await _gynecologistService.GetByMinimumOperationCountAsync(count);
            return Ok(result);
        }

        /// <summary>
        /// Отримує профіль гінеколога у вигляді текстового звіту.
        /// </summary>
        [Authorize(Roles = "Authorized, Operator, Admin")]
        [HttpGet("{id}/profile-summary")]
        public async Task<IActionResult> GetProfileSummary(int id)
        {
            var summary = await _gynecologistService.GetProfileSummaryAsync(id);

            if (summary.Contains("not found"))
                return NotFound(summary);

            return Ok(summary);
        }
    }
}