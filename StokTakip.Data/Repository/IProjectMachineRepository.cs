using StokTakip.Core.Dto;
using StokTakip.Core.Entity;

namespace StokTakip.Data.Repository;

public interface IProjectMachineRepository
{
    Task<ProjectMachine?> GetByIdAsync(int id);
    Task<IReadOnlyList<ProjectMachineListDto>> GetListAsync(int? projectId = null);
    Task<IReadOnlyList<ProjectMachineLookupDto>> GetLookupByProjectIdAsync(int projectId);
    Task<int> InsertAsync(ProjectMachine projectMachine);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int projectId, int machineId, int? excludeId = null);
    Task<bool> HasStockIssuesAsync(int projectMachineId);
}