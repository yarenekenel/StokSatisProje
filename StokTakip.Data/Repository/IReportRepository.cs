using StokTakip.Core.Dto;

namespace StokTakip.Data.Repository;

public interface IReportRepository
{
    Task<IReadOnlyList<StockBalanceDto>> GetStockBalanceAsync(int? productId, int? companyId, bool belowCriticalOnly);
    Task<IReadOnlyList<ProjectCostDto>> GetProjectCostSummaryAsync();
    Task<ProjectCostDto?> GetProjectCostByIdAsync(int projectId);
    Task<IReadOnlyList<ProjectCostDetailDto>> GetProjectCostDetailAsync(int projectId);
    Task<IReadOnlyList<ProjectCostDto>> GetProjectCostReportAsync(int? projectId, int? companyId, int? productId, DateTime? dateFrom, DateTime? dateTo);
}