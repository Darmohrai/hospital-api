using hospital_api.DTOs.Tracking;
using hospital_api.Models.Tracking;
using hospital_api.Repositories.Interfaces.Tracking;
using hospital_api.Services.Interfaces.Tracking;

namespace hospital_api.Services.Implementations.Tracking;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepo;
    // Ми можемо додати репозиторії пацієнтів/лікарів для валідації,
    // але для простоти CRUD-операцій поки обійдемось без них.

    public AppointmentService(IAppointmentRepository appointmentRepo)
    {
        _appointmentRepo = appointmentRepo;
    }

    public async Task<Appointment?> GetByIdAsync(int id)
    {
        return await _appointmentRepo.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Appointment>> GetAllAsync()
    {
        return await _appointmentRepo.GetAllAsync();
    }

    public async Task<Appointment> CreateAsync(CreateAppointmentDto dto)
    {
        if (dto.ClinicId == null && dto.HospitalId == null)
            throw new System.ArgumentException("Необхідно вказати ClinicId або HospitalId.");
            
        var appointment = new Appointment
        {
            VisitDateTime = dto.VisitDateTime,
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            ClinicId = dto.ClinicId,
            HospitalId = dto.HospitalId,
            Summary = dto.Summary
        };

        await _appointmentRepo.AddAsync(appointment);
        return appointment;
    }

    public async Task<bool> UpdateAsync(int id, CreateAppointmentDto dto)
    {
        var existing = await _appointmentRepo.GetByIdAsync(id);
        if (existing == null)
            return false;
            
        if (dto.ClinicId == null && dto.HospitalId == null)
            throw new System.ArgumentException("Необхідно вказати ClinicId або HospitalId.");

        // Оновлюємо поля
        existing.VisitDateTime = dto.VisitDateTime;
        existing.PatientId = dto.PatientId;
        existing.DoctorId = dto.DoctorId;
        existing.ClinicId = dto.ClinicId;
        existing.HospitalId = dto.HospitalId;
        existing.Summary = dto.Summary;

        await _appointmentRepo.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _appointmentRepo.GetByIdAsync(id);
        if (existing == null)
            return false;

        await _appointmentRepo.DeleteAsync(id);
        return true;
    }
}