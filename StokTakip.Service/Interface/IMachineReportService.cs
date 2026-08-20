using StokTakip.Core.Dto;
using StokTakip.Core.Result;

namespace StokTakip.Service.Interface;

public interface IMachineReportService
{
    Task<Result<IReadOnlyList<MachineConsumptionReportDto>>> GetConsumptionAsync();
    Task<Result<IReadOnlyList<MachineCostReportDto>>> GetCostAsync();
    Task<Result<IReadOnlyList<ProjectMachineDistributionReportDto>>> GetDistributionAsync();
    Task<Result<IReadOnlyList<MachineUsageSummaryReportDto>>> GetUsageSummaryAsync();
}