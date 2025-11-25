using System.ComponentModel.DataAnnotations.Schema;
using hospital_api.Models.ClinicAggregate;
using hospital_api.Models.PatientAggregate;
using hospital_api.Models.StaffAggregate;

namespace hospital_api.Models.Tracking;

public class ClinicDoctorAssignment
{
    [ForeignKey(nameof(Patient))]
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    [ForeignKey(nameof(Doctor))]
    public int DoctorId { get; set; }
    public Staff Doctor { get; set; } = null!;

    [ForeignKey(nameof(Clinic))]
    public int ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;
}