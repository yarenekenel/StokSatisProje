using StokTakip.Core.Dto;

namespace StokTakip.Data.Repository;

public interface IMachineReportRepository
{
    Task<IReadOnlyList<MachineConsumptionReportDto>> GetConsumptionAsync();
    Task<IReadOnlyList<MachineCostReportDto>> GetCostAsync();
    Task<IReadOnlyList<ProjectMachineDistributionReportDto>> GetDistributionAsync();
    Task<IReadOnlyList<MachineUsageSummaryReportDto>> GetUsageSummaryAsync();
}