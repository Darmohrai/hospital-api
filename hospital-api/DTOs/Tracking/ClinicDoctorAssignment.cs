using System.ComponentModel.DataAnnotations;

namespace hospital_api.DTOs.Tracking;

// DTO для створення та видалення зв'язку M:M
public class ClinicDoctorAssignmentDto
{
    [Required]
    public int PatientId { get; set; }
    
    [Required]
    public int DoctorId { get; set; }
    
    [Required]
    public int ClinicId { get; set; }
}