using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hospital_api.Models.HospitalAggregate;

public class Department
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Specialization { get; set; } = string.Empty;

    [ForeignKey(nameof(Building))]
    public int BuildingId { get; set; }

    public Building Building { get; set; } = null!;

    public List<Room> Rooms { get; set; } = new();
}
