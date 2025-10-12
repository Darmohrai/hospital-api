using hospital_api.Models.HospitalAggregate;

namespace hospital_api.Utils;

public static class SpecializationMapper
{
    public static bool TryGetSpecialization(string specialty, out HospitalSpecialization mappedSpecialization)
    {
        switch (specialty.ToLower())
        {
            case "neurologist":
                mappedSpecialization = HospitalSpecialization.Neurologist;
                return true;
            case "surgeon":
                mappedSpecialization = HospitalSpecialization.Surgeon;
                return true;
            case "ophthalmologist":
                mappedSpecialization = HospitalSpecialization.Ophthalmologist;
                return true;
            case "dentist":
                mappedSpecialization = HospitalSpecialization.Dentist;
                return true;
            case "radiologist":
                mappedSpecialization = HospitalSpecialization.Radiologist;
                return true;
            case "gynecologist":
                mappedSpecialization = HospitalSpecialization.Gynecologist;
                return true;
            case "cardiologist":
                mappedSpecialization = HospitalSpecialization.Cardiologist;
                return true;
            default:
                mappedSpecialization = default;
                return false;
        }
    }
}