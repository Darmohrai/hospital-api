using hospital_api.Models.StaffAggregate;

namespace hospital_api.DTOs.Reports;

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
    public SupportRole Role { get; set; }
    public int WorkExperienceYears { get; set; }
}