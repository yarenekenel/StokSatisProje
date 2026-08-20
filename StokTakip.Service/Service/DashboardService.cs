using StokTakip.Core.Dto;
using StokTakip.Core.Result;
using StokTakip.Data.Repository;
using StokTakip.Service.Interface;

namespace StokTakip.Service.Service;

public sealed class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repository;

    public DashboardService(IDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<DashboardSummaryDto>> GetSummaryAsync()
    {
        var summary = await _repository.GetSummaryAsync();
        return Result.Success(summary);
    }

    public async Task<Result<IReadOnlyList<ProductStockChartDto>>> GetTopStockProductsAsync()
    {
        var products = await _repository.GetTopStockProductsAsync();
        return Result.Success(products);
    }

    public async Task<Result<IReadOnlyList<RecentActivityDto>>> GetRecentActivitiesAsync()
    {
        var activities = await _repository.GetRecentActivitiesAsync();
        return Result.Success(activities);
    }

    public async Task<Result<IReadOnlyList<StockTypeDistributionDto>>> GetStockByTypeAsync()
    {
        var distribution = await _repository.GetStockByTypeAsync();
        return Result.Success(distribution);
    }

    public async Task<Result<IReadOnlyList<CriticalStockDto>>> GetCriticalStockProductsAsync()
    {
        var products = await _repository.GetCriticalStockProductsAsync();
        return Result.Success(products);
    }
}