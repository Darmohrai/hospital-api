using hospital_api.Models.OperationsAggregate;

namespace hospital_api.Models.StaffAggregate.DoctorAggregate;

public class Dentist : Doctor
{
    public List<Operation> Operations { get; set; } = new(); // тільки якщо роблять операції

    public int OperationCount => Operations.Count;
    public int FatalOperationCount => Operations.Count(op => op.IsFatal);

    public float HazardPayCoefficient { get; set; }
}