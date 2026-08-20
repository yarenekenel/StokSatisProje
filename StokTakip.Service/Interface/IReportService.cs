using StokTakip.Core.Dto;
using StokTakip.Core.Result;

namespace StokTakip.Service.Interface;

public interface IReportService
{
    Task<Result<IReadOnlyList<StockBalanceDto>>> GetStockBalanceAsync(int? productId, int? companyId, bool belowCriticalOnly);
    Task<Result<IReadOnlyList<ProjectCostDto>>> GetProjectCostSummaryAsync();
    Task<Result<ProjectCostDto>> GetProjectCostAsync(int projectId);
    Task<Result<IReadOnlyList<ProjectCostDetailDto>>> GetProjectCostDetailAsync(int projectId);
    Task<Result<IReadOnlyList<ProjectCostDto>>> GetProjectCostReportAsync(int? projectId, int? companyId, int? productId, DateTime? dateFrom, DateTime? dateTo);
}