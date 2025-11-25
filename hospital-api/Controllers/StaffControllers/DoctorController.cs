using hospital_api.Models.StaffAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers
{
    [Authorize]
    [ApiController]
    [Route("api/staff/doctors")]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? specialty, 
            [FromQuery] AcademicDegree? degree, 
            [FromQuery] AcademicTitle? title)
        {
            var doctors = await _doctorService.GetAllAsync(specialty, degree, title);
            return Ok(doctors);
        }

        [Authorize(Roles = "Authorized, Operator, Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var doctor = await _doctorService.GetByIdAsync(id);
            if (doctor == null)
                return NotFound();

            return Ok(doctor);
        }

        [Authorize(Roles = "Authorized, Operator, Admin")]
        [HttpGet("by-specialty")]
        public async Task<IActionResult> GetBySpecialty([FromQuery] string specialty)
        {
            var doctors = await _doctorService.GetBySpecialtyAsync(specialty);
            return Ok(doctors);
        }

        [Authorize(Roles = "Authorized, Operator, Admin")]
        [HttpGet("by-degree")]
        public async Task<IActionResult> GetByDegree([FromQuery] AcademicDegree degree)
        {
            var doctors = await _doctorService.GetByDegreeAsync(degree);
            return Ok(doctors);
        }

        [Authorize(Roles = "Authorized, Operator, Admin")]
        [HttpGet("by-title")]
        public async Task<IActionResult> GetByTitle([FromQuery] AcademicTitle title)
        {
            var doctors = await _doctorService.GetByTitleAsync(title);
            return Ok(doctors);
        }
    }
}