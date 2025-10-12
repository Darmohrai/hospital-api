using hospital_api.Models.OperationsAggregate;

namespace hospital_api.Models.StaffAggregate.DoctorAggregate;

public class Dentist : Doctor
{
    public List<Operation> Operations { get; set; } = new(); // тільки якщо роблять операції

    public int OperationCount
    {
        get => Operations.Count;
        init => throw new NotImplementedException();
    }

    public int FatalOperationCount
    {
        get => Operations.Count(op => op.IsFatal);
        init => throw new NotImplementedException();
    }

    public float HazardPayCoefficient { get; set; }
}