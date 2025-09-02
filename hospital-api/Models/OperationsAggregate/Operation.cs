using System;
using System.ComponentModel.DataAnnotations;
using hospital_api.Models.ClinicAggregate;
using hospital_api.Models.HospitalAggregate;
using hospital_api.Models.PatientAggregate;
using hospital_api.Models.StaffAggregate;

namespace hospital_api.Models.OperationsAggregate;

public class Operation
{
    public int Id { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;

    public bool IsFatal { get; set; }

    // Пацієнт, якому зробили операцію
    public int PatientId { get; set; }
    public Patient Patient { get; set; }

    // Лікар, що провів операцію
    public int DoctorId { get; set; }
    public Staff Doctor { get; set; }

    // Можлива лікарня, де була операція
    public int? HospitalId { get; set; }
    public Hospital? Hospital { get; set; }

    // Можлива клініка, де була операція
    public int? ClinicId { get; set; }
    public Clinic? Clinic { get; set; }
}