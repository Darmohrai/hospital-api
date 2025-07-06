using System;

namespace hospital_api.Models.PatientAggregate;

public class Patient
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string HealthStatus { get; set; }
    public float Temperature { get; set; }
    
    public int? HospitalId { get; set; }
    public HospitalAggregate.Hospital Hospital { get; set; }

    public int? ClinicId { get; set; }
    public ClinicAggregate.Clinic Clinic { get; set; }

    public int? AssignedDoctorId { get; set; }
    public StaffAggregate.Staff AssignedDoctor { get; set; }
}