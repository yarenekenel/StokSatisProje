using StokTakip.Core.Dto;
using StokTakip.Core.Entity;
using StokTakip.Core.Result;

namespace StokTakip.Service.Interface;

public interface IProjectService
{
    Task<Result<IReadOnlyList<ProjectListDto>>> GetListAsync(bool? isActive = null);
    Task<Result<Project>> GetByIdAsync(int id);
    Task<Result<int>> CreateAsync(string projectCode, int companyId, decimal? offerAmount, int? currencyId, decimal? exchangeRate, DateTime? startDate, string? description);
    Task<Result> UpdateAsync(int id, string projectCode, int companyId, decimal? offerAmount, int? currencyId, decimal? exchangeRate, DateTime? startDate, string? description);
    Task<Result> SetActiveAsync(int id, bool isActive);
}