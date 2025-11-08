using hospital_api.Models.ClinicAggregate;
using hospital_api.Models.HospitalAggregate;

namespace hospital_api.Models.StaffAggregate;

public class Employment
{
    public int Id { get; set; }

    public int StaffId { get; set; }
    public Staff Staff { get; set; } = null!;
    
    // Співробітник може працювати АБО в лікарні, АБО в клініці
    public int? HospitalId { get; set; }
    public Hospital? Hospital { get; set; }

    public int? ClinicId { get; set; }
    public Clinic? Clinic { get; set; }
}