using Entities;

namespace Contracts;

public interface IWorkShiftService
{
    public Task<ICollection<WorkShift>> GetWorkShiftsAsync();
    public Task<WorkShift> GetWorkShiftByIdAsync(long? id);
    public Task CreateWorkShiftAsync(WorkShift workShift);
    public Task RemoveWorkShiftAsync(long id);
    public Task UpdateWorkShiftAsync(WorkShift workShift);
    public Task AddGuardToWorkShiftAsync(string guardId, string shiftId);
    public Task RemoveGuardFromWorkShift(string guardId, string shiftId);
}