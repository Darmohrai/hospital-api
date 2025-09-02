using System;
using System.ComponentModel.DataAnnotations;
using hospital_api.Models.ClinicAggregate;
using hospital_api.Models.HospitalAggregate;
using hospital_api.Models.StaffAggregate;

namespace hospital_api.Models.PatientAggregate;

public class Patient
{
    public int Id { get; set; }

    [Required]
    public string FullName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string HealthStatus { get; set; } = string.Empty;

    public float Temperature { get; set; }

    // Поліклініка (обов’язково)
    [Required]
    public int ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;

    // Лікарня (опціонально, тільки за направленням)
    public int? HospitalId { get; set; }
    public Hospital? Hospital { get; set; }

    // Призначений лікар (може бути відсутній)
    public int? AssignedDoctorId { get; set; }
    public Staff? AssignedDoctor { get; set; }
}