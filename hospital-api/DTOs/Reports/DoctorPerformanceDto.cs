namespace hospital_api.DTOs.Reports;

public class DoctorPerformanceDto
{
    public int DoctorId { get; set; }
    public string DoctorFullName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public int TotalOperations { get; set; }
    public int FatalOperations { get; set; }
    public double FatalityRatePercent { get; set; }
}