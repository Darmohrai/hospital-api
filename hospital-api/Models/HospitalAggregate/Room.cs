namespace hospital_api.Models.HospitalAggregate;

public class Room
{
    public int Id { get; set; }
    public string Number { get; set; }
    public int Capacity { get; set; }

    public int DepartmentId { get; set; }
    public Department Department { get; set; }

    public List<Bed> Beds { get; set; }
}