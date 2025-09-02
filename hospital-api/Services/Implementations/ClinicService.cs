using hospital_api.Data;
using hospital_api.Models.ClinicAggregate;
using hospital_api.Models.HospitalAggregate;
using hospital_api.Models.PatientAggregate;
using hospital_api.Models.StaffAggregate;
using hospital_api.Repositories.Interfaces;
using hospital_api.Services.Interfaces;

namespace hospital_api.Services.Implementations;

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ClinicService : IClinicService
{
    private readonly IClinicRepository _clinicRepository;
    private readonly ApplicationDbContext _context;

    public ClinicService(IClinicRepository clinicRepository, ApplicationDbContext context)
    {
        _clinicRepository = clinicRepository;
        _context = context;
    }

    public async Task<IEnumerable<Clinic>> GetAllClinicsAsync()
        => await _clinicRepository.GetAllAsync();

    public async Task<Clinic?> GetClinicByIdAsync(int id)
        => await _clinicRepository.GetByIdAsync(id);

    public async Task<Clinic> CreateClinicAsync(Clinic clinic)
    {
        await _clinicRepository.AddAsync(clinic);
        return clinic;
    }

    public async Task<Clinic> UpdateClinicAsync(Clinic clinic)
    {
        await _clinicRepository.UpdateAsync(clinic);
        return clinic;
    }

    public async Task DeleteClinicAsync(int id)
        => await _clinicRepository.DeleteAsync(id);

    public async Task AssignHospitalAsync(int clinicId, int hospitalId)
    {
        var clinic = await _clinicRepository.GetByIdAsync(clinicId);
        if (clinic == null) throw new KeyNotFoundException("Clinic not found");

        var hospital = await _context.Hospitals.FindAsync(hospitalId);
        if (hospital == null) throw new KeyNotFoundException("Hospital not found");

        clinic.HospitalId = hospital.Id;
        await _clinicRepository.UpdateAsync(clinic);
    }

    public async Task AddStaffToClinicAsync(int clinicId, Staff staff)
    {
        var clinic = await _clinicRepository.GetByIdAsync(clinicId);
        if (clinic == null) throw new KeyNotFoundException("Clinic not found");

        clinic.Staff.Add(staff);
        await _clinicRepository.UpdateAsync(clinic);
    }

    public async Task AddPatientAsync(int clinicId, Patient patient)
    {
        var clinic = await _clinicRepository.GetByIdAsync(clinicId);
        if (clinic == null) throw new KeyNotFoundException("Clinic not found");

        patient.ClinicId = clinic.Id;
        clinic.Patients.Add(patient);

        await _context.Patients.AddAsync(patient);
        await _context.SaveChangesAsync();
    }

    public async Task<Hospital?> ReferPatientToHospitalAsync(int patientId, HospitalSpecialization requiredSpecialization)
    {
        var patient = await _context.Patients
            .Include(p => p.Clinic)
            .ThenInclude(c => c.Hospital)
            .FirstOrDefaultAsync(p => p.Id == patientId);

        if (patient == null)
            throw new KeyNotFoundException("Patient not found");

        // Лікарня, до якої приписана клініка
        var defaultHospital = patient.Clinic?.Hospital;

        // Перевіряємо, чи рідна лікарня має потрібну спеціалізацію
        if (defaultHospital != null && defaultHospital.Specializations.Contains(requiredSpecialization))
        {
            patient.HospitalId = defaultHospital.Id;
            patient.Hospital = defaultHospital;
        }
        else
        {
            // Шукаємо альтернативну лікарню
            var alternative = await _context.Hospitals
                .FirstOrDefaultAsync(h => h.Specializations.Contains(requiredSpecialization));

            if (alternative == null)
                return null;

            patient.HospitalId = alternative.Id;
            patient.Hospital = alternative;
        }

        await _context.SaveChangesAsync();
        return patient.Hospital;
    }

}
