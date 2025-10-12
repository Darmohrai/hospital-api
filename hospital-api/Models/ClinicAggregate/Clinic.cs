using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using hospital_api.Models.HospitalAggregate;
using hospital_api.Models.PatientAggregate;
using hospital_api.Models.StaffAggregate;

namespace hospital_api.Models.ClinicAggregate;

public class Clinic
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    [ForeignKey(nameof(Hospital))]
    public int? HospitalId { get; set; }
    public Hospital? Hospital { get; set; }

    public List<Employment> Employments { get; set; } = new();

    public List<Patient> Patients { get; set; } = new();
}