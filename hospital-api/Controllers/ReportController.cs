using hospital_api.DTOs.Reports;
using hospital_api.Models.StaffAggregate;
using hospital_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers;

[Authorize(Roles = "Authorized, Operator, Admin")]
[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("daily-appointments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailyAppointmentCount(
        [FromQuery] DateTime date,
        [FromQuery] int? doctorId,
        [FromQuery] int? clinicId,
        [FromQuery] string? specialty)
    {
        var reportDate = date.Date;

        var report = await _reportService.GetDailyAppointmentCountAsync(reportDate, doctorId, clinicId, specialty);
        return Ok(report);
    }

    [HttpGet("hospital-capacity/{hospitalId}")]
    [ProducesResponseType(typeof(HospitalCapacityReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHospitalCapacity(int hospitalId)
    {
        try
        {
            var report = await _reportService.GetHospitalCapacityReportAsync(hospitalId);
            return Ok(report);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("laboratory-report")]
    [ProducesResponseType(typeof(LaboratoryReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLaboratoryReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? hospitalId,
        [FromQuery] int? clinicId)
    {
        if (startDate > endDate)
            return BadRequest("startDate не може бути пізнішою за endDate.");

        var report = await _reportService.GetLaboratoryReportAsync(startDate, endDate, hospitalId, clinicId);
        return Ok(report);
    }

    [HttpGet("patient-operations")]
    [ProducesResponseType(typeof(IEnumerable<PatientOperationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientOperationReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? hospitalId,
        [FromQuery] int? clinicId,
        [FromQuery] int? doctorId)
    {
        if (startDate > endDate)
            return BadRequest("startDate не може бути пізнішою за endDate.");

        if (hospitalId.HasValue && clinicId.HasValue)
            return BadRequest("Вкажіть АБО hospitalId, АБО clinicId, але не обидва.");

        var report =
            await _reportService.GetPatientOperationReportAsync(startDate, endDate, hospitalId, clinicId, doctorId);
        return Ok(report);
    }

    [HttpGet("patients-by-clinic-specialty")]
    [ProducesResponseType(typeof(IEnumerable<PatientListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientsByClinicAndSpecialty(
        [FromQuery] int clinicId,
        [FromQuery] string specialty)
    {
        if (string.IsNullOrEmpty(specialty))
            return BadRequest("Параметр 'specialty' є обов'язковим.");

        var patients = await _reportService.GetPatientsByClinicAndSpecialtyAsync(clinicId, specialty);
        return Ok(patients);
    }

    [HttpGet("inpatient-report")]
    [ProducesResponseType(typeof(IEnumerable<PatientListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInpatientReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? hospitalId,
        [FromQuery] int? doctorId)
    {
        if (startDate > endDate)
            return BadRequest("startDate не може бути пізнішою за endDate.");

        var patients = await _reportService.GetInpatientReportAsync(startDate, endDate, hospitalId, doctorId);
        return Ok(patients);
    }

    [HttpGet("doctor-report")]
    [ProducesResponseType(typeof(DoctorReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDoctorReport(
        [FromQuery] int? hospitalId,
        [FromQuery] int? clinicId,
        [FromQuery] string? specialty,
        [FromQuery] int? minWorkExperience,
        [FromQuery] AcademicDegree? degree,
        [FromQuery] AcademicTitle? title)
    {
        if (hospitalId.HasValue && clinicId.HasValue)
            return BadRequest("Вкажіть АБО hospitalId, АБО clinicId, але не обидва.");

        var report =
            await _reportService.GetDoctorReportAsync(hospitalId, clinicId, specialty, minWorkExperience, degree,
                title);
        return Ok(report);
    }
    
    [HttpGet("doctor-operation-report")]
    [ProducesResponseType(typeof(DoctorReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDoctorOperationReport(
        [FromQuery] int minOperationCount,
        [FromQuery] string? specialty,
        [FromQuery] int? hospitalId,
        [FromQuery] int? clinicId)
    {
        if (hospitalId.HasValue && clinicId.HasValue)
            return BadRequest("Вкажіть АБО hospitalId, АБО clinicId, але не обидва.");

        var report =
            await _reportService.GetDoctorOperationReportAsync(minOperationCount, specialty, hospitalId, clinicId);
        return Ok(report);
    }

    [HttpGet("support-staff-report")]
    [ProducesResponseType(typeof(SupportStaffReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupportStaffReport(
        [FromQuery] SupportRole role,
        [FromQuery] int? hospitalId,
        [FromQuery] int? clinicId)
    {
        if (hospitalId.HasValue && clinicId.HasValue)
            return BadRequest("Вкажіть АБО hospitalId, АБО clinicId, але не обидва.");

        var report = await _reportService.GetSupportStaffReportAsync(role, hospitalId, clinicId);
        return Ok(report);
    }
    
    [HttpGet("doctor-performance-report")]
    [ProducesResponseType(typeof(IEnumerable<DoctorPerformanceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDoctorPerformanceReport(
        [FromQuery] int? hospitalId,
        [FromQuery] int? clinicId)
    {
        if (hospitalId.HasValue && clinicId.HasValue)
            return BadRequest("Вкажіть АБО hospitalId, АБО clinicId, але не обидва.");

        var report = await _reportService.GetDoctorPerformanceReportAsync(hospitalId, clinicId);
        return Ok(report);
    }
    
    [HttpGet("department-occupancy/{departmentId}")]
    [ProducesResponseType(typeof(DepartmentOccupancyReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDepartmentOccupancy(int departmentId)
    {
        try
        {
            var report = await _reportService.GetDepartmentOccupancyReportAsync(departmentId);
            return Ok(report);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}