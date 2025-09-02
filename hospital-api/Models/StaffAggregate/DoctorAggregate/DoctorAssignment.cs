using System.ComponentModel.DataAnnotations;
using hospital_api.Models.ClinicAggregate;
using hospital_api.Models.HospitalAggregate;

namespace hospital_api.Models.StaffAggregate.DoctorAggregate;

public class DoctorAssignment
{

    [Key] public int DoctorAssignmentId { get; set; }

    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; }

    public int? ClinicId { get; set; }
    public Clinic? Clinic { get; set; }

    public int? HospitalId { get; set; }
    public Hospital? Hospital { get; set; }

    public bool IsConsulting { get; set; } // якщо працює як консультант
}