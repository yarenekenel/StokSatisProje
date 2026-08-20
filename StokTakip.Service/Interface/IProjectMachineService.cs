using StokTakip.Core.Dto;
using StokTakip.Core.Result;

namespace StokTakip.Service.Interface;

public interface IProjectMachineService
{
    Task<Result<IReadOnlyList<ProjectMachineListDto>>> GetListAsync(int? projectId = null);
    Task<Result<IReadOnlyList<ProjectMachineLookupDto>>> GetLookupByProjectIdAsync(int projectId);
    Task<Result<int>> AssignAsync(int projectId, int machineId);
    Task<Result> UnassignAsync(int id);
}