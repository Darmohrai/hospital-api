using hospital_api.DTOs.Staff;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers
{
    [Authorize]
    [ApiController]
    [Route("api/staff/dentists")] // Більш чіткий та REST-сумісний маршрут
    public class DentistController : ControllerBase
    {
        private readonly IDentistService _dentistService;

        public DentistController(IDentistService dentistService)
        {
            _dentistService = dentistService;
        }

        /// <summary>
        /// Отримує список всіх стоматологів.
        /// </summary>
        [Authorize(Roles = "Authorized, Operator, Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var dentists = await _dentistService.GetAllAsync();
            return Ok(dentists);
        }

        /// <summary>
        /// Отримує стоматолога за його ID.
        /// </summary>
        [Authorize(Roles = "Authorized, Operator, Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dentist = await _dentistService.GetByIdAsync(id);
            if (dentist == null)
                return NotFound();

            return Ok(dentist);
        }

        /// <summary>
        /// Створює нового стоматолога.
        /// </summary>
        [Authorize(Roles = "Operator, Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDentistDto dto)
        {
            var dentist = new Dentist
            {
                FullName = dto.FullName,
                WorkExperienceYears = dto.WorkExperienceYears,
                AcademicDegree = dto.AcademicDegree,
                AcademicTitle = dto.AcademicTitle,
                OperationCount = dto.OperationCount,
                FatalOperationCount = dto.FatalOperationCount,
                HazardPayCoefficient = dto.HazardPayCoefficient
            };

            var result = await _dentistService.CreateAsync(dentist);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.ErrorMessage });

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }

        /// <summary>
        /// Оновлює дані існуючого стоматолога.
        /// </summary>
        [Authorize(Roles = "Operator, Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Dentist dentist)
        {
            if (id != dentist.Id)
                return BadRequest("ID mismatch.");

            var result = await _dentistService.UpdateAsync(dentist);

            if (!result.IsSuccess)
                return NotFound(new { message = result.ErrorMessage });

            return NoContent();
        }

        /// <summary>
        /// Видаляє стоматолога за його ID.
        /// </summary>
        [Authorize(Roles = "Operator, Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _dentistService.DeleteAsync(id);

            if (!result.IsSuccess)
                return NotFound(new { message = result.ErrorMessage });

            return NoContent();
        }

        /// <summary>
        /// Отримує стоматологів з кількістю операцій не менше вказаної.
        /// </summary>
        [Authorize(Roles = "Authorized, Operator, Admin")]
        [HttpGet("min-operations")]
        public async Task<IActionResult> GetByMinimumOperationCount([FromQuery] int minOperationCount)
        {
            var dentists = await _dentistService.GetByMinimumOperationCountAsync(minOperationCount);
            return Ok(dentists);
        }

        /// <summary>
        /// Отримує стоматологів з коефіцієнтом шкідливості не менше вказаного.
        /// </summary>
        [Authorize(Roles = "Authorized, Operator, Admin")]
        [HttpGet("high-hazard-pay")]
        public async Task<IActionResult> GetByHazardPay([FromQuery] float minCoefficient)
        {
            var dentists = await _dentistService.GetByHazardPayCoefficientAsync(minCoefficient);
            return Ok(dentists);
        }

        /// <summary>
        /// Отримує профіль стоматолога у вигляді текстового звіту.
        /// </summary>
        [Authorize(Roles = "Authorized, Operator, Admin")]
        [HttpGet("{id}/summary")]
        public async Task<IActionResult> GetSummary(int id)
        {
            var summary = await _dentistService.GetSummaryAsync(id);

            if (summary.Contains("not found"))
                return NotFound(summary);

            return Ok(summary);
        }
    }
}