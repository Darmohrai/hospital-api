using System;

namespace hospital_api.Models.OperationsAggregate;

public class Operation
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; }
    public bool IsFatal { get; set; }

    public int PatientId { get; set; }
    public PatientAggregate.Patient Patient { get; set; }

    public int DoctorId { get; set; }
    public StaffAggregate.Staff Doctor { get; set; }

    public int? HospitalId { get; set; }
    public HospitalAggregate.Hospital Hospital { get; set; }

    public int? ClinicId { get; set; }
    public ClinicAggregate.Clinic Clinic { get; set; }
}