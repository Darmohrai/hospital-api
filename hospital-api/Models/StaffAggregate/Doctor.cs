using System.ComponentModel.DataAnnotations;
using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Models.StaffAggregate;

public abstract class Doctor : Staff
{
    [Required] public string Specialty { get; set; } = string.Empty;
    
    [Required] public AcademicDegree? AcademicDegree { get; set; }
    [Required] public AcademicTitle? AcademicTitle { get; set; }

    public List<DoctorAssignment> Assignments { get; set; } = new();
}

public enum AcademicDegree
{
    None,
    Candidate,
    Doctor
}

public enum AcademicTitle
{
    None,
    AssociateProfessor,
    Professor
}
