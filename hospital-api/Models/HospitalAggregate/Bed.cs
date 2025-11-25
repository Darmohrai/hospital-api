using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using hospital_api.Models.PatientAggregate;

namespace hospital_api.Models.HospitalAggregate;

public class Bed
{
    [Key] public int Id { get; set; }

    [Required] public bool IsOccupied { get; set; }
    
    [ForeignKey(nameof(Room))] public int RoomId { get; set; }
    
    public Room? Room { get; set; }
    
    [ForeignKey(nameof(Patient))]
    public int? PatientId { get; set; }
    public Patient? Patient { get; set; }
}