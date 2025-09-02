namespace hospital_api.Models.StaffAggregate.DoctorAggregate;

public class Radiologist : Doctor
{
    public float HazardPayCoefficient { get; set; }
    public int ExtendedVacationDays { get; set; }
}