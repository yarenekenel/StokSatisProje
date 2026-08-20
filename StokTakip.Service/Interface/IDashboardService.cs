using StokTakip.Core.Dto;
using StokTakip.Core.Result;

namespace StokTakip.Service.Interface;

public interface IDashboardService
{
    Task<Result<DashboardSummaryDto>> GetSummaryAsync();
    Task<Result<IReadOnlyList<ProductStockChartDto>>> GetTopStockProductsAsync();
    Task<Result<IReadOnlyList<RecentActivityDto>>> GetRecentActivitiesAsync();
    Task<Result<IReadOnlyList<StockTypeDistributionDto>>> GetStockByTypeAsync();
    Task<Result<IReadOnlyList<CriticalStockDto>>> GetCriticalStockProductsAsync();
}