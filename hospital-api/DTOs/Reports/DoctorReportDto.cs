using hospital_api.Models.StaffAggregate; // <-- ✅ ДОДАЙ ЦЕЙ USING
using System.Collections.Generic;

namespace hospital_api.DTOs.Reports;

// DTO для Запиту №8
public class DoctorReportDto
{
    public int TotalCount { get; set; }
    public string FilterDescription { get; set; } = string.Empty;
    public List<DoctorDetails> Doctors { get; set; } = new();
}

public class DoctorDetails
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int WorkExperienceYears { get; set; }
    public string Specialty { get; set; } = string.Empty;
    public AcademicDegree? AcademicDegree { get; set; }
    public AcademicTitle? AcademicTitle { get; set; }
}