using System;
using System.ComponentModel.DataAnnotations;

namespace hospital_api.DTOs.Tracking;

public class CreateAdmissionDto
{
    [Required]
    public DateTime AdmissionDate { get; set; }

    [Required]
    public int PatientId { get; set; }

    [Required]
    public int HospitalId { get; set; }

    [Required]
    public int AttendingDoctorId { get; set; }

    // Відділення, куди госпіталізують
    public int? DepartmentId { get; set; }
}