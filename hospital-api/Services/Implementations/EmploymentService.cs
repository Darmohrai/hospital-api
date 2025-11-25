using hospital_api.DTOs.Staff;
using hospital_api.Models.StaffAggregate;
using hospital_api.Repositories.Interfaces;
using hospital_api.Repositories.Interfaces.HospitalRepo;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Services.Implementations
{
    public class EmploymentService : IEmploymentService
    {
        private readonly IEmploymentRepository _employmentRepo;
        private readonly IStaffRepository _staffRepo;
        private readonly IHospitalRepository _hospitalRepo;
        private readonly IClinicRepository _clinicRepo;

        public EmploymentService(
            IEmploymentRepository employmentRepo,
            IStaffRepository staffRepo,
            IHospitalRepository hospitalRepo,
            IClinicRepository clinicRepo)
        {
            _employmentRepo = employmentRepo;
            _staffRepo = staffRepo;
            _hospitalRepo = hospitalRepo;
            _clinicRepo = clinicRepo;
        }

        public async Task<ServiceResponse<IEnumerable<Employment>>> GetEmploymentsByStaffIdAsync(int staffId)
        {
            var employments = await _employmentRepo.GetEmploymentsByStaffIdAsync(staffId);
            return ServiceResponse<IEnumerable<Employment>>.Success(employments);
        }

        public async Task<ServiceResponse<Employment>> CreateEmploymentAsync(CreateEmploymentDto dto)
        {
            if (await _staffRepo.GetByIdAsync(dto.StaffId) == null)
            {
                return ServiceResponse<Employment>.Fail("Лікаря (Staff) з таким ID не знайдено.");
            }

            if (dto.HospitalId == null && dto.ClinicId == null)
            {
                return ServiceResponse<Employment>.Fail("Необхідно вказати HospitalId або ClinicId.");
            }

            if (dto.HospitalId != null && dto.ClinicId != null)
            {
                return ServiceResponse<Employment>.Fail("Вкажіть АБО HospitalId, АБО ClinicId, але не обидва.");
            }

            bool alreadyExists = false;
            if (dto.HospitalId.HasValue)
            {
                if (await _hospitalRepo.GetByIdAsync(dto.HospitalId.Value) == null)
                    return ServiceResponse<Employment>.Fail("Лікарні з таким ID не знайдено.");

                alreadyExists = await _employmentRepo.GetAll()
                    .AnyAsync(e => e.StaffId == dto.StaffId && e.HospitalId == dto.HospitalId);
            }
            else if (dto.ClinicId.HasValue)
            {
                if (await _clinicRepo.GetByIdAsync(dto.ClinicId.Value) == null)
                    return ServiceResponse<Employment>.Fail("Клініки з таким ID не знайдено.");

                alreadyExists = await _employmentRepo.GetAll()
                    .AnyAsync(e => e.StaffId == dto.StaffId && e.ClinicId == dto.ClinicId);
            }

            if (alreadyExists)
            {
                return ServiceResponse<Employment>.Fail("Цей лікар вже призначений у цей заклад.");
            }

            var employment = new Employment
            {
                StaffId = dto.StaffId,
                HospitalId = dto.HospitalId,
                ClinicId = dto.ClinicId
            };

            await _employmentRepo.AddAsync(employment);

            return ServiceResponse<Employment>.Success(employment);
        }

        public async Task<ServiceResponse<bool>> DeleteEmploymentAsync(int employmentId)
        {
            var employment = await _employmentRepo.GetByIdAsync(employmentId);
            if (employment == null)
            {
                return ServiceResponse<bool>.Fail("Запис про працевлаштування не знайдено.");
            }

            await _employmentRepo.DeleteAsync(employmentId);
            return ServiceResponse<bool>.Success(true);
        }
    }
}