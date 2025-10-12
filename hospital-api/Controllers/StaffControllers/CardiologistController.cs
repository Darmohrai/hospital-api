using hospital_api.DTOs.Staff;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers
{
    [ApiController]
    [Route("api/staff/cardiologists")] // Більш чіткий та REST-сумісний маршрут
    public class CardiologistController : ControllerBase
    {
        private readonly ICardiologistService _cardiologistService;

        public CardiologistController(ICardiologistService cardiologistService)
        {
            _cardiologistService = cardiologistService;
        }

        /// <summary>
        /// Отримує список всіх кардіологів.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cardiologists = await _cardiologistService.GetAllAsync();
            return Ok(cardiologists);
        }

        /// <summary>
        /// Отримує кардіолога за його ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cardiologist = await _cardiologistService.GetByIdAsync(id);
            if (cardiologist == null)
                return NotFound();

            return Ok(cardiologist);
        }

        /// <summary>
        /// Створює нового кардіолога.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCardiologistDto dto)
        {
            var cardiologist = new Cardiologist
            {
                FullName = dto.FullName,
                WorkExperienceYears = dto.WorkExperienceYears,
                AcademicDegree = dto.AcademicDegree,
                AcademicTitle = dto.AcademicTitle,
                OperationCount = dto.OperationCount,
                FatalOperationCount = dto.FatalOperationCount
            };

            var result = await _cardiologistService.CreateAsync(cardiologist);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.ErrorMessage });

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }

        /// <summary>
        /// Оновлює дані існуючого кардіолога.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Cardiologist cardiologist)
        {
            if (id != cardiologist.Id)
                return BadRequest("ID mismatch.");

            var result = await _cardiologistService.UpdateAsync(cardiologist);

            if (!result.IsSuccess)
                return NotFound(new { message = result.ErrorMessage });

            return NoContent();
        }

        /// <summary>
        /// Видаляє кардіолога за його ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _cardiologistService.DeleteAsync(id);

            if (!result.IsSuccess)
                return NotFound(new { message = result.ErrorMessage });

            return NoContent();
        }

        /// <summary>
        /// Отримує кардіологів з кількістю операцій не менше вказаної.
        /// </summary>
        [HttpGet("min-operations")]
        public async Task<IActionResult> GetByMinimumOperationCount([FromQuery] int minOperations)
        {
            var cardiologists = await _cardiologistService.GetByMinimumOperationCountAsync(minOperations);
            return Ok(cardiologists);
        }

        /// <summary>
        /// Отримує профіль кардіолога у вигляді текстового звіту.
        /// </summary>
        [HttpGet("{id}/profile-summary")]
        public async Task<IActionResult> GetProfileSummary(int id)
        {
            var summary = await _cardiologistService.GetProfileSummaryAsync(id);

            if (summary.Contains("not found"))
                return NotFound(summary);

            return Ok(summary);
        }
    }
}