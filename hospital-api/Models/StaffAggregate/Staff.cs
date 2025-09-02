using System.ComponentModel.DataAnnotations;
using hospital_api.Models.ClinicAggregate;
using hospital_api.Models.HospitalAggregate;

namespace hospital_api.Models.StaffAggregate;

public abstract class Staff
{
    [Key] public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int WorkExperienceYears { get; set; }
    
    public int? ClinicId { get; set; }
    public Clinic? Clinic { get; set; }

    public int? HospitalId { get; set; }
    public Hospital? Hospital { get; set; }
}