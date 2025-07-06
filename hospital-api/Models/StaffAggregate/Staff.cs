namespace hospital_api.Models.StaffAggregate;

public class Staff
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Position { get; set; }
    public string Specialty { get; set; }
    public int WorkExperienceYears { get; set; }
    
    public string AcademicDegree { get; set; }
    public string AcademicTitle { get; set; }
    
    public int? OperationCount { get; set; }
    public int? FatalOperationCount { get; set; }
    
    public float? HazardPayCoefficient { get; set; }
    public int? ExtendedVacationDays { get; set; }
    
    public int? HospitalId { get; set; }
    public HospitalAggregate.Hospital Hospital { get; set; }

    public int? ClinicId { get; set; }
    public ClinicAggregate.Clinic Clinic { get; set; }
}