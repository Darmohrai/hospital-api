using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hospital_api.Models.HospitalAggregate;

public class Bed
{
    [Key] public int Id { get; set; }

    [Required] public bool IsOccupied { get; set; }

    // Foreign Key
    [ForeignKey(nameof(Room))] public int RoomId { get; set; }

    // Navigation Property
    public Room Room { get; set; } = null!;
}