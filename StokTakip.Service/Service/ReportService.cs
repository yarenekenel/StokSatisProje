using StokTakip.Core.Dto;
using StokTakip.Core.Result;
using StokTakip.Data.Repository;
using StokTakip.Service.Interface;

namespace StokTakip.Service.Service;

public sealed class ReportService : IReportService
{
    private readonly IReportRepository _repository;

    public ReportService(IReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<StockBalanceDto>>> GetStockBalanceAsync(int? productId, int? companyId, bool belowCriticalOnly)
    {
        var list = await _repository.GetStockBalanceAsync(productId, companyId, belowCriticalOnly);
        return Result.Success(list);
    }
    public async Task<Result<IReadOnlyList<ProjectCostDto>>> GetProjectCostSummaryAsync()
    {
        var list = await _repository.GetProjectCostSummaryAsync();
        return Result.Success(list);
    }

    public async Task<Result<ProjectCostDto>> GetProjectCostAsync(int projectId)
    {
        var cost = await _repository.GetProjectCostByIdAsync(projectId);
        if (cost is null)
            return Result.Failure<ProjectCostDto>(Error.NotFound("Proje bulunamadı."));

        return Result.Success(cost);
    }

    public async Task<Result<IReadOnlyList<ProjectCostDetailDto>>> GetProjectCostDetailAsync(int projectId)
    {
        var list = await _repository.GetProjectCostDetailAsync(projectId);
        return Result.Success(list);
    }

    public async Task<Result<IReadOnlyList<ProjectCostDto>>> GetProjectCostReportAsync(
        int? projectId, int? companyId, int? productId, DateTime? dateFrom, DateTime? dateTo)
    {
        var list = await _repository.GetProjectCostReportAsync(projectId, companyId, productId, dateFrom, dateTo);
        return Result.Success(list);
    }
}