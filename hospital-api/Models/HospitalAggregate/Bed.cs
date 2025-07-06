namespace hospital_api.Models.HospitalAggregate;

public class Bed
{
    public int Id { get; set; }
    public bool IsOccupied { get; set; }

    public int RoomId { get; set; }
    public Room Room { get; set; }
}