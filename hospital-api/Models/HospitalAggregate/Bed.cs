using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using hospital_api.Models.PatientAggregate;

namespace hospital_api.Models.HospitalAggregate;

public class Bed
{
    [Key] public int Id { get; set; }

    [Required] public bool IsOccupied { get; set; }

    // Foreign Key
    [ForeignKey(nameof(Room))] public int RoomId { get; set; }

    // Navigation Property
    public Room? Room { get; set; }

    // ✅ НОВІ ПОЛЯ: Зв'язок з пацієнтом (одне ліжко зайняте одним пацієнтом)
    [ForeignKey(nameof(Patient))]
    public int? PatientId { get; set; }
    public Patient? Patient { get; set; }
}