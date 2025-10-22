﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using hospital_api.Models.ClinicAggregate;
using hospital_api.Models.HospitalAggregate;
using hospital_api.Models.PatientAggregate;
using hospital_api.Models.StaffAggregate;

namespace hospital_api.Models.Tracking;

public class Appointment
{
    [Key] public int Id { get; set; }

    [Required] public DateTime VisitDateTime { get; set; }

    // --- Зв'язки ---

    [Required]
    [ForeignKey(nameof(Patient))]
    public int PatientId { get; set; }

    public Patient Patient { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(Doctor))]
    public int DoctorId { get; set; }

    public Staff Doctor { get; set; } = null!; // Використовуємо Staff, оскільки це загальний зв'язок

    // Місце візиту (може бути або клініка, або лікарня)
    [ForeignKey(nameof(Clinic))] public int? ClinicId { get; set; }
    public Clinic? Clinic { get; set; }

    [ForeignKey(nameof(Hospital))] public int? HospitalId { get; set; }
    public Hospital? Hospital { get; set; }

    // Нотатки лікаря за результатами візиту
    public string Summary { get; set; } = string.Empty;
}