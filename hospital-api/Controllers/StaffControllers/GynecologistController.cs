using hospital_api.DTOs.Staff;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers
{
    [Authorize]
    [ApiController]
    [Route("api/staff/gynecologists")]
    public class GynecologistController : ControllerBase
    {
        private readonly IGynecologistService _gynecologistService;

        public GynecologistController(IGynecologistService gynecologistService)
        {
            _gynecologistService = gynecologistService;
        }

        [Authorize(Roles = "Authorized, Operator, Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var gynecologists = await _gynecologistService.GetAllAsync();
            return Ok(gynecologists);
        }

        [Authorize(Roles = "Authorized, Operator, Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var gynecologist = await _gynecologistService.GetByIdAsync(id);
            if (gynecologist == null)
                return NotFound();

            return Ok(gynecologist);
        }

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

        [Authorize(Roles = "Operator, Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _gynecologistService.DeleteAsync(id);

            if (!result.IsSuccess)
                return NotFound(new { message = result.ErrorMessage });

            return NoContent();
        }

        [Authorize(Roles = "Authorized, Operator, Admin")]
        [HttpGet("min-operations/{count}")]
        public async Task<IActionResult> GetByMinimumOperationCount(int count)
        {
            var result = await _gynecologistService.GetByMinimumOperationCountAsync(count);
            return Ok(result);
        }

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