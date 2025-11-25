using hospital_api.Models.OperationsAggregate;

namespace hospital_api.Models.StaffAggregate.DoctorAggregate;

public class Dentist : Doctor
{
    public List<Operation> Operations { get; set; } = new();

    public int OperationCount
    {
        get => Operations.Count;
    }

    public int FatalOperationCount
    {
        get => Operations.Count(op => op.IsFatal);
    }

    public float HazardPayCoefficient { get; set; }
}