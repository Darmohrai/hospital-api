using System.ComponentModel.DataAnnotations.Schema;

namespace hospital_api.Models.HospitalAggregate;

using System.ComponentModel.DataAnnotations;

public class Building
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [ForeignKey(nameof(Hospital))]
    public int HospitalId { get; set; }

    public Hospital? Hospital { get; set; }
    public List<Department> Departments { get; set; } = new();
}
