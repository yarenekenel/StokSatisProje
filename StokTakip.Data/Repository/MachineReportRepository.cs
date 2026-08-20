using StokTakip.Core.Dto;
using StokTakip.Data.Context;

namespace StokTakip.Data.Repository;

public sealed class MachineReportRepository : BaseRepository, IMachineReportRepository
{
    public MachineReportRepository(IDapperContext context) : base(context)
    {
    }

    public Task<IReadOnlyList<MachineConsumptionReportDto>> GetConsumptionAsync()
    {
        const string sql = """
            SELECT m.[kod] AS MachineCode,
                   m.[ad] AS MachineName,
                   u.Kod AS ProductCode,
                   u.Ad AS ProductName,
                   SUM(c.Miktar) AS Quantity
            FROM tx_StokCikis c
            JOIN tx_ProjeMakina pm ON pm.Id = c.ProjeMakinaId
            JOIN dt_Makina m ON m.[id] = pm.MakinaId
            JOIN tx_StokGiris e ON e.Id = c.StokGirisId
            JOIN dt_Urun u ON u.Id = e.UrunId
            GROUP BY m.[kod], m.[ad], u.Kod, u.Ad
            ORDER BY m.[kod], u.Kod
            """;
        return QueryAsync<MachineConsumptionReportDto>(sql);
    }

    public Task<IReadOnlyList<MachineCostReportDto>> GetCostAsync()
    {
        const string sql = """
            SELECT m.[kod] AS MachineCode,
                   m.[ad] AS MachineName,
                   p.ProjeKodu AS ProjectCode,
                   SUM(c.TutarTL) AS TotalCostTry
            FROM tx_StokCikis c
            JOIN tx_ProjeMakina pm ON pm.Id = c.ProjeMakinaId
            JOIN dt_Makina m ON m.[id] = pm.MakinaId
            JOIN dt_Proje p ON p.Id = pm.ProjeId
            GROUP BY m.[kod], m.[ad], p.ProjeKodu
            ORDER BY m.[kod], p.ProjeKodu
            """;
        return QueryAsync<MachineCostReportDto>(sql);
    }

    public Task<IReadOnlyList<ProjectMachineDistributionReportDto>> GetDistributionAsync()
    {
        const string sql = """
            SELECT p.ProjeKodu AS ProjectCode,
                   m.[kod] AS MachineCode,
                   m.[ad] AS MachineName,
                   pm.AtamaTarihi AS AssignedAt
            FROM tx_ProjeMakina pm
            JOIN dt_Proje p ON p.Id = pm.ProjeId
            JOIN dt_Makina m ON m.[id] = pm.MakinaId
            ORDER BY p.ProjeKodu, m.[kod]
            """;
        return QueryAsync<ProjectMachineDistributionReportDto>(sql);
    }

    public Task<IReadOnlyList<MachineUsageSummaryReportDto>> GetUsageSummaryAsync()
    {
        const string sql = """
            SELECT m.[kod] AS MachineCode,
                   m.[ad] AS MachineName,
                   COUNT(DISTINCT pm.ProjeId) AS ProjectCount,
                   ISNULL(SUM(c.Miktar), 0) AS TotalQuantity,
                   ISNULL(SUM(c.TutarTL), 0) AS TotalCostTry
            FROM dt_Makina m
            LEFT JOIN tx_ProjeMakina pm ON pm.MakinaId = m.[id]
            LEFT JOIN tx_StokCikis c ON c.ProjeMakinaId = pm.Id
            GROUP BY m.[kod], m.[ad]
            ORDER BY m.[kod]
            """;
        return QueryAsync<MachineUsageSummaryReportDto>(sql);
    }
}