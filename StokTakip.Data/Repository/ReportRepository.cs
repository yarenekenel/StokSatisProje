using StokTakip.Core.Dto;
using StokTakip.Data.Context;

namespace StokTakip.Data.Repository;

public sealed class ReportRepository : BaseRepository, IReportRepository
{
    public ReportRepository(IDapperContext context) : base(context)
    {
    }

    public Task<IReadOnlyList<StockBalanceDto>> GetStockBalanceAsync(int? productId, int? companyId, bool belowCriticalOnly)
    {
        const string sql = """
        SELECT v.UrunId AS ProductId,
               v.UrunKodu AS ProductCode,
               v.UrunAdi AS ProductName,
               v.Birim AS Unit,
               v.ToplamGiris AS TotalIn,
               v.ToplamCikis AS TotalOut,
               v.KalanStok AS RemainingStock,
               v.KritikStokSeviyesi AS CriticalStockLevel
        FROM vw_StokBakiye v
        WHERE (@ProductId IS NULL OR v.UrunId = @ProductId)
          AND (@CompanyId IS NULL OR EXISTS (
                SELECT 1 FROM tx_StokGiris e
                WHERE e.UrunId = v.UrunId AND e.FirmaId = @CompanyId
          ))
          AND (@BelowCriticalOnly = 0 OR (v.KritikStokSeviyesi IS NOT NULL AND v.KalanStok < v.KritikStokSeviyesi))
        ORDER BY v.UrunKodu
        """;
        return QueryAsync<StockBalanceDto>(sql, new { ProductId = productId, CompanyId = companyId, BelowCriticalOnly = belowCriticalOnly });
    }

    private const string ProjectCostBaseSql = """
        SELECT p.Id AS ProjectId,
               p.ProjeKodu AS ProjectCode,
               f.FirmaAdi AS CompanyName,
               p.TeklifTutariTL AS OfferAmountTry,
               ISNULL(SUM(c.TutarTL), 0) AS TotalIssueCostTry,
               p.TeklifTutariTL - ISNULL(SUM(c.TutarTL), 0) AS EstimatedProfitTry
        FROM dt_Proje p
        JOIN lu_Firma f ON f.Id = p.FirmaId
        LEFT JOIN tx_ProjeMakina pm ON pm.ProjeId = p.Id
        LEFT JOIN tx_StokCikis c ON c.ProjeMakinaId = pm.Id
        """;

    public Task<IReadOnlyList<ProjectCostDto>> GetProjectCostSummaryAsync()
    {
        const string sql = ProjectCostBaseSql + """
            GROUP BY p.Id, p.ProjeKodu, f.FirmaAdi, p.TeklifTutariTL
            ORDER BY p.ProjeKodu
            """;
        return QueryAsync<ProjectCostDto>(sql);
    }

    public Task<ProjectCostDto?> GetProjectCostByIdAsync(int projectId)
    {
        const string sql = ProjectCostBaseSql + """
             WHERE p.Id = @ProjectId
            GROUP BY p.Id, p.ProjeKodu, f.FirmaAdi, p.TeklifTutariTL
            """;
        return QueryFirstOrDefaultAsync<ProjectCostDto>(sql, new { ProjectId = projectId });
    }

    public Task<IReadOnlyList<ProjectCostDetailDto>> GetProjectCostDetailAsync(int projectId)
    {
        const string sql = """
            SELECT pm.Id AS ProjectMachineId,
                   m.[kod] AS MachineCode,
                   m.[ad] AS MachineName,
                   c.Id AS StockIssueId,
                   u.Kod AS ProductCode,
                   u.Ad AS ProductName,
                   b.Ad AS UnitName,
                   e.MalKabulNo AS GoodsReceiptNo,
                   e.FaturaNo AS InvoiceNo,
                   c.Miktar AS Quantity,
                   c.BirimFiyat AS UnitPrice,
                   c.TutarTL AS AmountTry,
                   c.Tarih AS Date,
                   c.Aciklama AS Description
            FROM tx_ProjeMakina pm
            JOIN dt_Makina m ON m.[id] = pm.MakinaId
            LEFT JOIN tx_StokCikis c ON c.ProjeMakinaId = pm.Id
            LEFT JOIN tx_StokGiris e ON e.Id = c.StokGirisId
            LEFT JOIN dt_Urun u ON u.Id = e.UrunId
            LEFT JOIN lu_Birim b ON b.Id = u.BirimId
            WHERE pm.ProjeId = @ProjectId
            ORDER BY m.[kod], c.Tarih, c.Id
            """;
        return QueryAsync<ProjectCostDetailDto>(sql, new { ProjectId = projectId });
    }
    public Task<IReadOnlyList<ProjectCostDto>> GetProjectCostReportAsync(
        int? projectId, int? companyId, int? productId, DateTime? dateFrom, DateTime? dateTo)
    {
        const string sql = """
            SELECT p.Id AS ProjectId,
                   p.ProjeKodu AS ProjectCode,
                   f.FirmaAdi AS CompanyName,
                   p.TeklifTutariTL AS OfferAmountTry,
                   ISNULL(SUM(c.TutarTL), 0) AS TotalIssueCostTry,
                   p.TeklifTutariTL - ISNULL(SUM(c.TutarTL), 0) AS EstimatedProfitTry
            FROM dt_Proje p
            JOIN lu_Firma f ON f.Id = p.FirmaId
            LEFT JOIN tx_ProjeMakina pm ON pm.ProjeId = p.Id
            LEFT JOIN tx_StokCikis c
                ON c.ProjeMakinaId = pm.Id
               AND (@DateFrom IS NULL OR CAST(c.Tarih AS date) >= @DateFrom)
               AND (@DateTo IS NULL OR CAST(c.Tarih AS date) <= @DateTo)
            WHERE (@ProjectId IS NULL OR p.Id = @ProjectId)
              AND (@CompanyId IS NULL OR p.FirmaId = @CompanyId)
              AND (@ProductId IS NULL OR EXISTS (
                    SELECT 1
                    FROM tx_StokCikis cx
                    JOIN tx_ProjeMakina pmx ON pmx.Id = cx.ProjeMakinaId
                    JOIN tx_StokGiris ex ON ex.Id = cx.StokGirisId
                    WHERE pmx.ProjeId = p.Id
                      AND ex.UrunId = @ProductId
                      AND (@DateFrom IS NULL OR CAST(cx.Tarih AS date) >= @DateFrom)
                      AND (@DateTo IS NULL OR CAST(cx.Tarih AS date) <= @DateTo)
              ))
            GROUP BY p.Id, p.ProjeKodu, f.FirmaAdi, p.TeklifTutariTL
            ORDER BY f.FirmaAdi, p.ProjeKodu
            """;
        return QueryAsync<ProjectCostDto>(sql, new { ProjectId = projectId, CompanyId = companyId, ProductId = productId, DateFrom = dateFrom, DateTo = dateTo });
    }
}