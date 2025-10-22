using hospital_api.Models.StaffAggregate; // <-- ✅ ДОДАЙ ЦЕЙ USING (для SupportRole)
using System.Collections.Generic;

namespace hospital_api.DTOs.Reports;

// DTO для Запиту №10
public class SupportStaffReportDto
{
    public int TotalCount { get; set; }
    public string FilterDescription { get; set; } = string.Empty;
    public List<SupportStaffDetails> Staff { get; set; } = new();
}

public class SupportStaffDetails
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public SupportRole Role { get; set; } // "Спеціальність"
    public int WorkExperienceYears { get; set; }
}