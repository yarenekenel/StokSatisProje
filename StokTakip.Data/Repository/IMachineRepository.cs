using StokTakip.Core.Entity;
using StokTakip.Data.Context;

namespace StokTakip.Data.Repository;

public interface IMachineRepository
{
    Task<IReadOnlyList<Machine>> GetAllAsync(bool? isActive = null);
    Task<Machine?> GetByIdAsync(int id);
    Task<int> InsertAsync(Machine machine);
    Task<bool> UpdateAsync(Machine machine);
    Task<bool> SetActiveAsync(int id, bool isActive);
    Task<bool> ExistsByCodeAsync(string code, int? excludeId = null);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<int> GetNextSequenceForProjectAsync(int projectId);
    Task<bool> HasProjectLinksAsync(int machineId);
}