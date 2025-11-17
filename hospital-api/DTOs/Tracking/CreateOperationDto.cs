using System.ComponentModel.DataAnnotations;

namespace hospital_api.DTOs.Tracking;

public class CreateOperationDto
{
    [Required]
    public DateTime Date { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;

    public bool IsFatal { get; set; }

    [Required]
    public int PatientId { get; set; }

    [Required]
    public int DoctorId { get; set; }

    public int? HospitalId { get; set; }
    public int? ClinicId { get; set; }
}