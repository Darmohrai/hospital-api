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

    public int PatientId { get; set; }
    public Patient Patient { get; set; }

    public int DoctorId { get; set; }
    public Staff Doctor { get; set; }

    public int? HospitalId { get; set; }
    public Hospital? Hospital { get; set; }

    public int? ClinicId { get; set; }
    public Clinic? Clinic { get; set; }
}