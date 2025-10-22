using hospital_api.DTOs.Tracking;
using hospital_api.Models.Tracking;

namespace hospital_api.Services.Interfaces.Tracking;

public interface IAppointmentService
{
    Task<Appointment?> GetByIdAsync(int id);
    Task<IEnumerable<Appointment>> GetAllAsync();
    Task<Appointment> CreateAsync(CreateAppointmentDto dto);
    Task<bool> UpdateAsync(int id, CreateAppointmentDto dto);
    Task<bool> DeleteAsync(int id);
}