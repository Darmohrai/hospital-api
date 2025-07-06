namespace hospital_api.Models.LaboratoryAggregate;

public class Laboratory
{
    public int Id { get; set; }
    public string Name { get; set; }

    // Profiles: біохімічні, фізіологічні, хімічні
    public string Profile { get; set; }

    // Договори з лікарнями/поліклініками
    public List<HospitalAggregate.Hospital> Hospitals { get; set; }
    public List<ClinicAggregate.Clinic> Clinics { get; set; }
}