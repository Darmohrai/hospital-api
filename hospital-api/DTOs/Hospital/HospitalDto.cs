using hospital_api.Models.HospitalAggregate;
using hospital_api.Models.StaffAggregate;

namespace hospital_api.DTOs.Hospital;

public class HospitalDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    
    
    public List<Building> Buildings { get; set; }
    
    public List<Department> Departments { get; set; }
    
    public List<Models.PatientAggregate.Patient> Patients { get; set; }

    public List<Models.ClinicAggregate.Clinic> Clinics { get; set; }
    public List<HospitalSpecialization> Specializations { get; set; }
}