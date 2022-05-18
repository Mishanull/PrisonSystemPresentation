using Entities;

namespace Contracts;

public interface IWorkShiftService
{
    public Task<ICollection<WorkShift>> GetWorkShifts();
    public Task<WorkShift> GetWorkShiftById(long id);
    public Task CreateWorkShiftAsync(WorkShift workShift);
    public Task RemoveWorkShiftAsync(long id);
    public Task UpdateWorkShiftAsync(WorkShift workShift);
}