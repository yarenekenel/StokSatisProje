using StokTakip.Core.Dto;
using StokTakip.Core.Result;
using StokTakip.Data.Repository;
using StokTakip.Service.Interface;

namespace StokTakip.Service.Service;

public sealed class MachineReportService : IMachineReportService
{
    private readonly IMachineReportRepository _repository;

    public MachineReportService(IMachineReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<MachineConsumptionReportDto>>> GetConsumptionAsync()
    {
        var list = await _repository.GetConsumptionAsync();
        return Result.Success(list);
    }

    public async Task<Result<IReadOnlyList<MachineCostReportDto>>> GetCostAsync()
    {
        var list = await _repository.GetCostAsync();
        return Result.Success(list);
    }

    public async Task<Result<IReadOnlyList<ProjectMachineDistributionReportDto>>> GetDistributionAsync()
    {
        var list = await _repository.GetDistributionAsync();
        return Result.Success(list);
    }

    public async Task<Result<IReadOnlyList<MachineUsageSummaryReportDto>>> GetUsageSummaryAsync()
    {
        var list = await _repository.GetUsageSummaryAsync();
        return Result.Success(list);
    }
}