using hospital_api.Models.OperationsAggregate;

namespace hospital_api.Models.StaffAggregate.DoctorAggregate;

public class Gynecologist : Doctor
{
    public List<Operation> Operations { get; set; } = new();

    public int OperationCount => Operations.Count;
    public int FatalOperationCount => Operations.Count(op => op.IsFatal);
}