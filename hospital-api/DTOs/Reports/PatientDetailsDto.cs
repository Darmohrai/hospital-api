namespace hospital_api.DTOs.Patient;

public class PatientDetailsDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string HealthStatus { get; set; } = string.Empty;
    public float Temperature { get; set; }
    
    public int? AttendingDoctorId { get; set; }
    public string AttendingDoctorName { get; set; } = string.Empty;
    
    public int? BedId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
}