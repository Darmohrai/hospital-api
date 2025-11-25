using System.ComponentModel.DataAnnotations;

namespace hospital_api.DTOs.Tracking;

public class CreateAppointmentDto
{
    [Required]
    public DateTime VisitDateTime { get; set; }

    [Required]
    public int PatientId { get; set; }

    [Required]
    public int DoctorId { get; set; }

    public int? ClinicId { get; set; }
    public int? HospitalId { get; set; }
    
    [Required]
    public string Summary { get; set; } = string.Empty;
}