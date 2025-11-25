using hospital_api.DTOs.Clinic;
using hospital_api.Models.ClinicAggregate;
using hospital_api.Models.HospitalAggregate;
using hospital_api.Models.PatientAggregate;
using hospital_api.Models.StaffAggregate;
using hospital_api.Repositories.Interfaces;
using hospital_api.Repositories.Interfaces.HospitalRepo;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces;
using hospital_api.Utils;

namespace hospital_api.Services.Implementations;

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ClinicService : IClinicService
{
    private readonly IClinicRepository _clinicRepository;
    private readonly IHospitalRepository _hospitalRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IEmploymentRepository _employmentRepository;
    private readonly IPatientRepository _patientRepository;

    public ClinicService(
        IClinicRepository clinicRepository,
        IHospitalRepository hospitalRepository,
        IStaffRepository staffRepository,
        IEmploymentRepository employmentRepository,
        IPatientRepository patientRepository)
    {
        _clinicRepository = clinicRepository;
        _hospitalRepository = hospitalRepository;
        _staffRepository = staffRepository;
        _employmentRepository = employmentRepository;
        _patientRepository = patientRepository;
    }

    public async Task<IEnumerable<Clinic>> GetAllAsync() => await _clinicRepository.GetAllAsync();
    public async Task<Clinic?> GetByIdAsync(int id) => await _clinicRepository.GetByIdAsync(id);
    public async Task CreateAsync(Clinic clinic) => await _clinicRepository.AddAsync(clinic);
    public async Task UpdateAsync(Clinic clinic) => await _clinicRepository.UpdateAsync(clinic);
    public async Task DeleteAsync(int id) => await _clinicRepository.DeleteAsync(id);

    public async Task<ServiceResponse<Clinic>> AssignHospitalAsync(int clinicId, int hospitalId)
    {
        var clinic = await _clinicRepository.GetByIdAsync(clinicId);
        if (clinic == null)
            return ServiceResponse<Clinic>.Fail("Clinic not found.");

        var hospital = await _hospitalRepository.GetByIdAsync(hospitalId);
        if (hospital == null)
            return ServiceResponse<Clinic>.Fail("Hospital not found.");

        clinic.HospitalId = hospital.Id;
        await _clinicRepository.UpdateAsync(clinic);

        return ServiceResponse<Clinic>.Success(clinic);
    }

    public async Task<ServiceResponse<Employment>> AddStaffToClinicAsync(int clinicId, int staffId)
    {
        var clinic = await _clinicRepository.GetAll()
            .Include(c => c.Hospital)
            .FirstOrDefaultAsync(c => c.Id == clinicId);

        if (clinic == null)
            return ServiceResponse<Employment>.Fail("Clinic not found.");

        var staff = await _staffRepository.GetByIdAsync(staffId);
        if (staff == null)
            return ServiceResponse<Employment>.Fail("Staff not found.");

        if (staff is Doctor doctor)
        {
            if (clinic.Hospital == null)
                return ServiceResponse<Employment>.Fail(
                    "Cannot add a doctor to a clinic that is not attached to a hospital.");

            if (!SpecializationMapper.TryGetSpecialization(doctor.Specialty, out var requiredSpec))
                return ServiceResponse<Employment>.Fail($"Unknown specialty: {doctor.Specialty}");

            if (clinic.Hospital.Specializations == null || !clinic.Hospital.Specializations.Contains(requiredSpec))
                return ServiceResponse<Employment>.Fail(
                    $"The hospital '{clinic.Hospital.Name}' does not support the specialty '{doctor.Specialty}'.");
        }

        var employment = new Employment { ClinicId = clinicId, StaffId = staffId };
        await _employmentRepository.AddAsync(employment);

        return ServiceResponse<Employment>.Success(employment);
    }

    public async Task<ServiceResponse<Patient>> AddPatientAsync(int clinicId, Patient patient)
    {
        var clinic = await _clinicRepository.GetByIdAsync(clinicId);
        if (clinic == null)
            return ServiceResponse<Patient>.Fail("Clinic not found.");

        patient.ClinicId = clinic.Id;
        await _patientRepository.AddAsync(patient);

        return ServiceResponse<Patient>.Success(patient);
    }

    public async Task<ServiceResponse<Hospital>> ReferPatientToHospitalAsync(int patientId,
        HospitalSpecialization requiredSpecialization)
    {
        var patient = await _patientRepository.GetAll()
            .Include(p => p.Clinic)
            .ThenInclude(c => c!.Hospital)
            .FirstOrDefaultAsync(p => p.Id == patientId);

        if (patient == null)
            return ServiceResponse<Hospital>.Fail("Patient not found.");

        if (patient.Clinic?.Hospital != null &&
            patient.Clinic.Hospital.Specializations.Contains(requiredSpecialization))
        {
            patient.HospitalId = patient.Clinic.Hospital.Id;
            await _patientRepository.UpdateAsync(patient);
            return ServiceResponse<Hospital>.Success(patient.Clinic.Hospital);
        }

        var alternativeHospital = await _hospitalRepository.GetAll()
            .FirstOrDefaultAsync(h => h.Specializations.Contains(requiredSpecialization));

        if (alternativeHospital == null)
            return ServiceResponse<Hospital>.Fail($"No hospital found with specialization: {requiredSpecialization}");

        patient.HospitalId = alternativeHospital.Id;
        await _patientRepository.UpdateAsync(patient);

        return ServiceResponse<Hospital>.Success(alternativeHospital);
    }

    public async Task<IEnumerable<ClinicDto>> GetAllDtosAsync()
    {
        return await _clinicRepository.GetAll()
            .Select(c => new ClinicDto
            {
                Id = c.Id,
                Name = c.Name,
                Address = c.Address,
                HospitalId = c.HospitalId
            })
            .ToListAsync();
    }
}