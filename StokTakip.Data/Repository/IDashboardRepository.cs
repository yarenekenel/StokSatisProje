using StokTakip.Core.Dto;

namespace StokTakip.Data.Repository;

public interface IDashboardRepository
{
    Task<DashboardSummaryDto> GetSummaryAsync();
    Task<IReadOnlyList<ProductStockChartDto>> GetTopStockProductsAsync(int top = 8);
    Task<IReadOnlyList<RecentActivityDto>> GetRecentActivitiesAsync(int count = 8);
    Task<IReadOnlyList<StockTypeDistributionDto>> GetStockByTypeAsync();
    Task<IReadOnlyList<CriticalStockDto>> GetCriticalStockProductsAsync();
}