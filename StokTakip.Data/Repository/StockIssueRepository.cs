using StokTakip.Core.Dto;
using StokTakip.Core.Entity;
using StokTakip.Data.Context;

namespace StokTakip.Data.Repository;

public sealed class StockIssueRepository : BaseRepository, IStockIssueRepository
{
    private const string SelectSql = """
        SELECT Id,
               ProjeMakinaId AS ProjectMachineId,
               StokGirisId AS StockEntryId,
               Miktar AS Quantity,
               BirimFiyat AS UnitPrice,
               TutarTL AS AmountTry,
               Tarih AS Date,
               Aciklama AS Description
        FROM tx_StokCikis
        """;

    public StockIssueRepository(IDapperContext context) : base(context)
    {
    }

    public Task<StockIssue?> GetByIdAsync(long id)
        => QueryFirstOrDefaultAsync<StockIssue>(SelectSql + " WHERE Id = @Id", new { Id = id });

    public Task<IReadOnlyList<StockIssue>> GetAllAsync()
        => QueryAsync<StockIssue>(SelectSql + " ORDER BY Tarih DESC, Id DESC");

    public Task<IReadOnlyList<StockIssue>> GetByProjectIdAsync(int projectId)
    {
        const string sql = """
            SELECT c.Id,
                   c.ProjeMakinaId AS ProjectMachineId,
                   c.StokGirisId AS StockEntryId,
                   c.Miktar AS Quantity,
                   c.BirimFiyat AS UnitPrice,
                   c.TutarTL AS AmountTry,
                   c.Tarih AS Date,
                   c.Aciklama AS Description
            FROM tx_StokCikis c
            JOIN tx_ProjeMakina pm ON pm.Id = c.ProjeMakinaId
            WHERE pm.ProjeId = @ProjectId
            ORDER BY c.Tarih DESC, c.Id DESC
            """;
        return QueryAsync<StockIssue>(sql, new { ProjectId = projectId });
    }

    public Task<IReadOnlyList<StockIssueListDto>> GetListAsync()
    {
        const string sql = """
            SELECT c.Id,
                   p.ProjeKodu AS ProjectCode,
                   m.[kod] AS MachineCode,
                   m.[ad] AS MachineName,
                   c.StokGirisId AS StockEntryId,
                   u.Kod AS ProductCode,
                   u.Ad AS ProductName,
                   c.Miktar AS Quantity,
                   c.BirimFiyat AS UnitPrice,
                   c.TutarTL AS AmountTry,
                   c.Tarih AS Date,
                   e.FaturaNo AS InvoiceNo,
                   e.MalKabulNo AS GoodsReceiptNo
            FROM tx_StokCikis c
            JOIN tx_ProjeMakina pm ON pm.Id = c.ProjeMakinaId
            JOIN dt_Proje p ON p.Id = pm.ProjeId
            JOIN dt_Makina m ON m.[id] = pm.MakinaId
            JOIN tx_StokGiris e ON e.Id = c.StokGirisId
            JOIN dt_Urun u ON u.Id = e.UrunId
            ORDER BY c.Tarih DESC, c.Id DESC
            """;
        return QueryAsync<StockIssueListDto>(sql);
    }

    public async Task<long> InsertAsync(StockIssue stockIssue)
    {
        const string sql = """
            INSERT INTO tx_StokCikis (ProjeMakinaId, StokGirisId, Miktar, BirimFiyat, Tarih, Aciklama)
            VALUES (@ProjectMachineId, @StockEntryId, @Quantity, @UnitPrice, @Date, @Description);
            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
            """;
        return await ExecuteScalarAsync<long>(sql, new
        {
            stockIssue.ProjectMachineId,
            stockIssue.StockEntryId,
            stockIssue.Quantity,
            stockIssue.UnitPrice,
            stockIssue.Date,
            stockIssue.Description
        });
    }
}