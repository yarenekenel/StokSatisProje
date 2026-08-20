using StokTakip.Core.Entity;
using StokTakip.Core.Result;

namespace StokTakip.Service.Interface;

public interface IMachineService
{
    Task<Result<IReadOnlyList<Machine>>> GetAllAsync(bool? isActive = null);
    Task<Result<Machine>> GetByIdAsync(int id);
    Task<Result<Machine>> CreateForProjectAsync(int projectId, string name);
    Task<Result> UpdateAsync(int id, string name);
    Task<Result> SetActiveAsync(int id, bool isActive);
}