namespace hospital_api.DTOs.Reports;

public class PatientListDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string HealthStatus { get; set; } = string.Empty;
}