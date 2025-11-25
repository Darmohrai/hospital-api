using hospital_api.DTOs.Staff;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers
{
    [Authorize]
    [ApiController]
    [Route("api/staff/cardiologists")]
    public class CardiologistController : ControllerBase
    {
        private readonly ICardiologistService _cardiologistService;

        public CardiologistController(ICardiologistService cardiologistService)
        {
            _cardiologistService = cardiologistService;
        }
        
        [Authorize(Roles = "Authorized, Operator, Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cardiologists = await _cardiologistService.GetAllAsync();
            return Ok(cardiologists);
        }

        [Authorize(Roles = "Authorized, Operator, Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cardiologist = await _cardiologistService.GetByIdAsync(id);
            if (cardiologist == null)
                return NotFound();

            return Ok(cardiologist);
        }

        [Authorize(Roles = "Operator, Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCardiologistDto dto)
        {
            var cardiologist = new Cardiologist
            {
                FullName = dto.FullName,
                WorkExperienceYears = dto.WorkExperienceYears,
                AcademicDegree = dto.AcademicDegree,
                AcademicTitle = dto.AcademicTitle,
            };

            var result = await _cardiologistService.CreateAsync(cardiologist);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.ErrorMessage });

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }

        [Authorize(Roles = "Operator, Admin")]
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

        [Authorize(Roles = "Operator, Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _cardiologistService.DeleteAsync(id);

            if (!result.IsSuccess)
                return NotFound(new { message = result.ErrorMessage });

            return NoContent();
        }

        [Authorize(Roles = "Authorized, Operator, Admin")]
        [HttpGet("min-operations")]
        public async Task<IActionResult> GetByMinimumOperationCount([FromQuery] int minOperations)
        {
            var cardiologists = await _cardiologistService.GetByMinimumOperationCountAsync(minOperations);
            return Ok(cardiologists);
        }

        [Authorize(Roles = "Authorized, Operator, Admin")]
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