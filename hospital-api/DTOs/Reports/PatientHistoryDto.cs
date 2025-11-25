namespace hospital_api.DTOs.Reports;

public class PatientHistoryDto
{
    public int PatientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string CurrentHealthStatus { get; set; } = string.Empty;
    
    public List<PatientHistoryEventDto> Events { get; set; } = new();
}

public class PatientHistoryEventDto
{
    public DateTime EventDate { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
}