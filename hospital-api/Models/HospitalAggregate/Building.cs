namespace hospital_api.Models.HospitalAggregate;

public class Building
{
    public int Id { get; set; }
    public string Name { get; set; }

    public int HospitalId { get; set; }
    public Hospital Hospital { get; set; }

    public List<Department> Departments { get; set; }
}