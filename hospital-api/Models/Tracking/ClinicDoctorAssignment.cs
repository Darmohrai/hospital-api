using System.ComponentModel.DataAnnotations.Schema;
using hospital_api.Models.ClinicAggregate;
using hospital_api.Models.PatientAggregate;
using hospital_api.Models.StaffAggregate;

namespace hospital_api.Models.Tracking;

// Ця модель реалізує зв'язок M:M "Пацієнт <-> Лікар" у поліклініці
public class ClinicDoctorAssignment
{
    // Композитний ключ буде налаштовано в DbContext
    
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