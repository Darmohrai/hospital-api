namespace hospital_api.Models.ClinicAggregate;

public class Clinic
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }

    // Може бути прикріплена до лікарні
    public int? HospitalId { get; set; }
    public HospitalAggregate.Hospital Hospital { get; set; }

    public List<StaffAggregate.Staff> Staff { get; set; }
    public List<PatientAggregate.Patient> Patients { get; set; }
}