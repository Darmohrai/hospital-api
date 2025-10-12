using hospital_api.Models.OperationsAggregate;

namespace hospital_api.Models.StaffAggregate.DoctorAggregate;

public class Surgeon : Doctor
{
    public List<Operation> Operations { get; set; } = new();

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
}