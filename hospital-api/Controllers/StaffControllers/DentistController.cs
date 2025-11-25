using hospital_api.DTOs.Staff;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers
{
    [Authorize]
    [ApiController]
    [Route("api/staff/dentists")]
    public class DentistController : ControllerBase
    {
        private readonly IDentistService _dentistService;

        public DentistController(IDentistService dentistService)
        {
            _dentistService = dentistService;
        }
        
        [Authorize(Roles = "Authorized, Operator, Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var dentists = await _dentistService.GetAllAsync();
            return Ok(dentists);
        }
        
        [Authorize(Roles = "Authorized, Operator, Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dentist = await _dentistService.GetByIdAsync(id);
            if (dentist == null)
                return NotFound();

            return Ok(dentist);
        }
        
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
            };

            var result = await _dentistService.CreateAsync(dentist);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.ErrorMessage });

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }

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

        [Authorize(Roles = "Operator, Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _dentistService.DeleteAsync(id);

            if (!result.IsSuccess)
                return NotFound(new { message = result.ErrorMessage });

            return NoContent();
        }

        [Authorize(Roles = "Authorized, Operator, Admin")]
        [HttpGet("min-operations")]
        public async Task<IActionResult> GetByMinimumOperationCount([FromQuery] int minOperationCount)
        {
            var dentists = await _dentistService.GetByMinimumOperationCountAsync(minOperationCount);
            return Ok(dentists);
        }

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