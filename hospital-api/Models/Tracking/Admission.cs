using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using hospital_api.Models.HospitalAggregate;
using hospital_api.Models.PatientAggregate;
using hospital_api.Models.StaffAggregate;

namespace hospital_api.Models.Tracking;

public class Admission
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime AdmissionDate { get; set; }
    
    public DateTime? DischargeDate { get; set; }

    [Required]
    [ForeignKey(nameof(Patient))]
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(Hospital))]
    public int HospitalId { get; set; }
    public Hospital Hospital { get; set; } = null!;
    
    [Required]
    [ForeignKey(nameof(AttendingDoctor))]
    public int AttendingDoctorId { get; set; }
    public Staff AttendingDoctor { get; set; } = null!;
    
    [ForeignKey(nameof(Department))]
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
}