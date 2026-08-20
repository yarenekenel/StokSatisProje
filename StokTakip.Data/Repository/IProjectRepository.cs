using StokTakip.Core.Dto;
using StokTakip.Core.Entity;

namespace StokTakip.Data.Repository;

public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> GetAllAsync(bool? isActive = null);
    Task<Project?> GetByIdAsync(int id);
    Task<IReadOnlyList<ProjectListDto>> GetListAsync(bool? isActive = null);
    Task<int> InsertAsync(Project project);
    Task<bool> UpdateAsync(Project project);
    Task<bool> SetActiveAsync(int id, bool isActive);
    Task<bool> ExistsByCodeAsync(string code, int? excludeId = null);
    Task<int> GetMaxCodeSequenceAsync();
    Task<bool> HasStockIssuesAsync(int projectId);
}