using hospital_api.Models.HospitalAggregate;
using hospital_api.Repositories.Interfaces.HospitalRepo;
using hospital_api.Services.Interfaces.HospitalServices;

namespace hospital_api.Services.Implementations.HospitalServices;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;

    public RoomService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<Room?> GetByIdAsync(int id)
    {
        return await _roomRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Room>> GetAllAsync()
    {
        return await _roomRepository.GetAllAsync();
    }

    public async Task AddAsync(Room room)
    {
        if (string.IsNullOrWhiteSpace(room.Number))
        {
            throw new ArgumentException("Room number is required.");
        }

        await _roomRepository.AddAsync(room);
    }

    public async Task UpdateAsync(Room room)
    {
        var existingRoom = await _roomRepository.GetByIdAsync(room.Id);
        if (existingRoom == null)
        {
            throw new InvalidOperationException("Room not found.");
        }

        await _roomRepository.UpdateAsync(room);
    }

    public async Task DeleteAsync(int id)
    {
        await _roomRepository.DeleteAsync(id);
    }

    public async Task<Room?> GetByNumberAsync(string roomNumber)
    {
        return await _roomRepository.GetByNumberAsync(roomNumber);
    }

    public async Task<IEnumerable<Room>> GetByDepartmentIdAsync(int departmentId)
    {
        return await _roomRepository.GetByDepartmentIdAsync(departmentId);
    }

    public async Task<IEnumerable<Room>> GetByCapacityAsync(int capacity)
    {
        return await _roomRepository.GetByCapacityAsync(capacity);
    }

    public async Task<IEnumerable<Room>> GetAllWithBedsAndDepartmentAsync()
    {
        return await _roomRepository.GetAllWithBedsAndDepartmentAsync();
    }
}
