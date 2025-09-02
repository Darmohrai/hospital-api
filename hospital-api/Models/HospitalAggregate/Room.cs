using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hospital_api.Models.HospitalAggregate;

public class Room
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Number { get; set; } = string.Empty;

    [Required]
    public int Capacity { get; set; }

    [ForeignKey(nameof(Department))]
    public int DepartmentId { get; set; }

    public Department Department { get; set; } = null!;

    public List<Bed> Beds { get; set; } = new();
}