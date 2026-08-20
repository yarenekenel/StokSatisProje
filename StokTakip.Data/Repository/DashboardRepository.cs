using StokTakip.Core.Dto;
using StokTakip.Data.Context;

namespace StokTakip.Data.Repository;

public sealed class DashboardRepository : BaseRepository, IDashboardRepository
{
    public DashboardRepository(IDapperContext context) : base(context)
    {
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        const string sql = """
            SELECT
                (SELECT COUNT(1) FROM dt_Urun WHERE Aktif = 1) AS ActiveProductCount,
                (SELECT COUNT(1) FROM lu_Firma WHERE Aktif = 1) AS ActiveCompanyCount,
                (SELECT COUNT(1) FROM dt_Proje WHERE Aktif = 1) AS ActiveProjectCount,
                (SELECT COUNT(1) FROM dt_Makina WHERE ISNULL([durum], 0) = 1) AS ActiveMachineCount,
                (SELECT ISNULL(SUM(KalanStok), 0) FROM vw_StokBakiye) AS TotalRemainingStock,
                (SELECT COUNT(1) FROM vw_StokBakiye WHERE KritikStokSeviyesi IS NOT NULL AND KalanStok < KritikStokSeviyesi) AS BelowCriticalStockCount
            """;

        var result = await QueryFirstOrDefaultAsync<DashboardSummaryDto>(sql);
        return result ?? new DashboardSummaryDto();
    }

    public Task<IReadOnlyList<ProductStockChartDto>> GetTopStockProductsAsync(int top = 8)
    {
        var sql = $"""
            SELECT TOP {top}
                   UrunKodu AS ProductCode,
                   UrunAdi AS ProductName,
                   KalanStok AS RemainingStock
            FROM vw_StokBakiye
            WHERE KalanStok > 0
            ORDER BY KalanStok DESC
            """;
        return QueryAsync<ProductStockChartDto>(sql);
    }

    public Task<IReadOnlyList<RecentActivityDto>> GetRecentActivitiesAsync(int count = 8)
    {
        var sql = $"""
            SELECT TOP {count} *
            FROM (
                SELECT 0 AS Type, u.Kod AS ProductCode, u.Ad AS ProductName, e.Miktar AS Quantity, e.Tarih AS Date
                FROM tx_StokGiris e
                JOIN dt_Urun u ON u.Id = e.UrunId

                UNION ALL

                SELECT 1 AS Type, u.Kod AS ProductCode, u.Ad AS ProductName, c.Miktar AS Quantity, c.Tarih AS Date
                FROM tx_StokCikis c
                JOIN tx_StokGiris e ON e.Id = c.StokGirisId
                JOIN dt_Urun u ON u.Id = e.UrunId
            ) activities
            ORDER BY Date DESC
            """;
        return QueryAsync<RecentActivityDto>(sql);
    }

    public Task<IReadOnlyList<StockTypeDistributionDto>> GetStockByTypeAsync()
    {
        const string sql = """
            SELECT s.Ad AS StockTypeName, ISNULL(SUM(v.KalanStok), 0) AS TotalRemainingStock
            FROM lu_StokTipi s
            LEFT JOIN dt_Urun u ON u.StokTipiId = s.Id
            LEFT JOIN vw_StokBakiye v ON v.UrunId = u.Id
            GROUP BY s.Ad
            HAVING ISNULL(SUM(v.KalanStok), 0) > 0
            ORDER BY TotalRemainingStock DESC
            """;
        return QueryAsync<StockTypeDistributionDto>(sql);
    }

    public Task<IReadOnlyList<CriticalStockDto>> GetCriticalStockProductsAsync()
    {
        const string sql = """
            SELECT UrunKodu AS ProductCode,
                   UrunAdi AS ProductName,
                   KalanStok AS RemainingStock,
                   KritikStokSeviyesi AS CriticalStockLevel,
                   Birim AS UnitName
            FROM vw_StokBakiye
            WHERE KritikStokSeviyesi IS NOT NULL
              AND KalanStok < KritikStokSeviyesi
            ORDER BY (KalanStok - KritikStokSeviyesi) ASC
            """;
        return QueryAsync<CriticalStockDto>(sql);
    }
}