using hospital_api.DTOs.Staff;
using hospital_api.Models.StaffAggregate;

namespace hospital_api.Services.Interfaces;

public interface IEmploymentService
{
    Task<ServiceResponse<Employment>> CreateEmploymentAsync(CreateEmploymentDto dto);
    Task<ServiceResponse<bool>> DeleteEmploymentAsync(int employmentId);
}