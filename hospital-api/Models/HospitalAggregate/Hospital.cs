namespace hospital_api.Models.HospitalAggregate;

public class Hospital
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    
    public List<Building> Buildings { get; set; }
    public List<Department> Departments { get; set; }
    public List<StaffAggregate.Staff> Staff { get; set; }
    public List<PatientAggregate.Patient> Patients { get; set; }
}