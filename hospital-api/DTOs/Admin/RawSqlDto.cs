using System.ComponentModel.DataAnnotations;

namespace hospital_api.DTOs.Admin;

public class RawSqlDto
{
    [Required]
    public string SqlQuery { get; set; } = string.Empty;
}