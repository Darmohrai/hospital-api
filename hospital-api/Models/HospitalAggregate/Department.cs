namespace hospital_api.Models.HospitalAggregate;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Specialization { get; set; }

    public int BuildingId { get; set; }
    public Building Building { get; set; }

    public List<Room> Rooms { get; set; }
}