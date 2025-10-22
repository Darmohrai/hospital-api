using System;

namespace hospital_api.DTOs.Patient;

// DTO для Запиту №4
public class PatientDetailsDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string HealthStatus { get; set; } = string.Empty;
    public float Temperature { get; set; }
    
    // Інформація про лікаря
    public int? AttendingDoctorId { get; set; }
    public string AttendingDoctorName { get; set; } = string.Empty;
    
    // Інформація про місцезнаходження
    public int? BedId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
}