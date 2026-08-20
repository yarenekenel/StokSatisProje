using StokTakip.Core.Dto;
using StokTakip.Core.Entity;
using StokTakip.Data.Context;

namespace StokTakip.Data.Repository;

public sealed class StockEntryRepository : BaseRepository, IStockEntryRepository
{
    private const string SelectSql = """
        SELECT Id,
               UrunId AS ProductId,
               FirmaId AS CompanyId,
               FaturaNo AS InvoiceNo,
               MalKabulNo AS GoodsReceiptNo,
               MalKabulYapan AS ReceivedBy,
               Miktar AS Quantity,
               BirimFiyat AS UnitPrice,
               ParaBirimiId AS CurrencyId,
               Kur AS ExchangeRate,
               TutarDoviz AS AmountForeign,
               TutarTL AS AmountTry,
               Tarih AS Date,
               Aciklama AS Description
        FROM tx_StokGiris
        """;

    public StockEntryRepository(IDapperContext context) : base(context)
    {
    }

    public Task<StockEntry?> GetByIdAsync(long id)
        => QueryFirstOrDefaultAsync<StockEntry>(SelectSql + " WHERE Id = @Id", new { Id = id });

    public Task<IReadOnlyList<StockEntry>> GetAllAsync()
        => QueryAsync<StockEntry>(SelectSql + " ORDER BY Tarih DESC, Id DESC");

    public Task<IReadOnlyList<StockEntryListDto>> GetListAsync()
    {
        const string sql = """
            SELECT e.Id,
                   u.Kod AS ProductCode,
                   u.Ad AS ProductName,
                   f.FirmaAdi AS CompanyName,
                   e.FaturaNo AS InvoiceNo,
                   e.MalKabulNo AS GoodsReceiptNo,
                   e.Miktar AS Quantity,
                   e.BirimFiyat AS UnitPrice,
                   p.Kod AS CurrencyCode,
                   e.TutarTL AS AmountTry,
                   e.Tarih AS Date
            FROM tx_StokGiris e
            JOIN dt_Urun u ON u.Id = e.UrunId
            LEFT JOIN lu_Firma f ON f.Id = e.FirmaId
            JOIN lu_ParaBirimi p ON p.Id = e.ParaBirimiId
            ORDER BY e.Tarih DESC, e.Id DESC
            """;
        return QueryAsync<StockEntryListDto>(sql);
    }

    public Task<IReadOnlyList<StockEntry>> GetByProductIdAsync(int productId)
        => QueryAsync<StockEntry>(SelectSql + " WHERE UrunId = @ProductId ORDER BY Tarih DESC, Id DESC", new { ProductId = productId });

    public Task<IReadOnlyList<StockEntryOpenLotDto>> GetOpenLotsByProductIdAsync(int productId)
    {
        const string sql = """
            SELECT e.Id,
                   e.UrunId AS ProductId,
                   e.Miktar AS Quantity,
                   e.TutarTL AS AmountTry,
                   e.Tarih AS Date,
                   e.FaturaNo AS InvoiceNo,
                   e.MalKabulNo AS GoodsReceiptNo,
                   e.Miktar - ISNULL(SUM(c.Miktar), 0) AS RemainingQuantity
            FROM tx_StokGiris e
            LEFT JOIN tx_StokCikis c ON c.StokGirisId = e.Id
            WHERE e.UrunId = @ProductId
            GROUP BY e.Id, e.UrunId, e.Miktar, e.TutarTL, e.Tarih, e.FaturaNo, e.MalKabulNo
            HAVING e.Miktar - ISNULL(SUM(c.Miktar), 0) > 0
            ORDER BY e.Tarih ASC, e.Id ASC
            """;
        return QueryAsync<StockEntryOpenLotDto>(sql, new { ProductId = productId });
    }

    public Task<StockEntryOpenLotDto?> GetOpenLotByIdAsync(long stockEntryId)
    {
        const string sql = """
            SELECT e.Id,
                   e.UrunId AS ProductId,
                   e.Miktar AS Quantity,
                   e.TutarTL AS AmountTry,
                   e.Tarih AS Date,
                   e.FaturaNo AS InvoiceNo,
                   e.MalKabulNo AS GoodsReceiptNo,
                   e.Miktar - ISNULL(SUM(c.Miktar), 0) AS RemainingQuantity
            FROM tx_StokGiris e
            LEFT JOIN tx_StokCikis c ON c.StokGirisId = e.Id
            WHERE e.Id = @StockEntryId
            GROUP BY e.Id, e.UrunId, e.Miktar, e.TutarTL, e.Tarih, e.FaturaNo, e.MalKabulNo
            HAVING e.Miktar - ISNULL(SUM(c.Miktar), 0) > 0
            """;
        return QueryFirstOrDefaultAsync<StockEntryOpenLotDto>(sql, new { StockEntryId = stockEntryId });
    }

    public Task<IReadOnlyList<StockLotLookupDto>> GetOpenLotsLookupAsync()
    {
        const string sql = """
            SELECT e.Id AS StockEntryId,
                   e.UrunId AS ProductId,
                   u.Kod AS ProductCode,
                   u.Ad AS ProductName,
                   b.Ad AS UnitName,
                   e.MalKabulNo AS GoodsReceiptNo,
                   e.FaturaNo AS InvoiceNo,
                   e.Tarih AS Date,
                   e.Miktar - ISNULL(SUM(c.Miktar), 0) AS RemainingQuantity
            FROM tx_StokGiris e
            JOIN dt_Urun u ON u.Id = e.UrunId
            JOIN lu_Birim b ON b.Id = u.BirimId
            LEFT JOIN tx_StokCikis c ON c.StokGirisId = e.Id
            WHERE u.Aktif = 1
            GROUP BY e.Id, e.UrunId, u.Kod, u.Ad, b.Ad, e.MalKabulNo, e.FaturaNo, e.Tarih, e.Miktar
            HAVING e.Miktar - ISNULL(SUM(c.Miktar), 0) > 0
            ORDER BY e.Tarih ASC, e.Id ASC
            """;
        return QueryAsync<StockLotLookupDto>(sql);
    }

    public async Task<long> InsertAsync(StockEntry stockEntry)
    {
        const string sql = """
            INSERT INTO tx_StokGiris
            (UrunId, FirmaId, FaturaNo, MalKabulNo, MalKabulYapan, Miktar, BirimFiyat, ParaBirimiId, Kur, Tarih, Aciklama)
            VALUES
            (@ProductId, @CompanyId, @InvoiceNo, @GoodsReceiptNo, @ReceivedBy, @Quantity, @UnitPrice, @CurrencyId, @ExchangeRate, @Date, @Description);
            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
            """;
        return await ExecuteScalarAsync<long>(sql, new
        {
            stockEntry.ProductId,
            stockEntry.CompanyId,
            stockEntry.InvoiceNo,
            stockEntry.GoodsReceiptNo,
            stockEntry.ReceivedBy,
            stockEntry.Quantity,
            stockEntry.UnitPrice,
            stockEntry.CurrencyId,
            stockEntry.ExchangeRate,
            stockEntry.Date,
            stockEntry.Description
        });
    }

    public async Task<bool> UpdateAsync(StockEntry stockEntry)
    {
        const string sql = """
            UPDATE tx_StokGiris
            SET UrunId = @ProductId,
                FirmaId = @CompanyId,
                FaturaNo = @InvoiceNo,
                MalKabulNo = @GoodsReceiptNo,
                MalKabulYapan = @ReceivedBy,
                Miktar = @Quantity,
                BirimFiyat = @UnitPrice,
                ParaBirimiId = @CurrencyId,
                Kur = @ExchangeRate,
                Tarih = @Date,
                Aciklama = @Description
            WHERE Id = @Id
            """;
        var affected = await ExecuteAsync(sql, new
        {
            stockEntry.Id,
            stockEntry.ProductId,
            stockEntry.CompanyId,
            stockEntry.InvoiceNo,
            stockEntry.GoodsReceiptNo,
            stockEntry.ReceivedBy,
            stockEntry.Quantity,
            stockEntry.UnitPrice,
            stockEntry.CurrencyId,
            stockEntry.ExchangeRate,
            stockEntry.Date,
            stockEntry.Description
        });
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = "DELETE FROM tx_StokGiris WHERE Id = @Id";
        var affected = await ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    public async Task<bool> HasIssuesAsync(long stockEntryId)
    {
        const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM tx_StokCikis WHERE StokGirisId = @Id) THEN 1 ELSE 0 END";
        var exists = await ExecuteScalarAsync<int>(sql, new { Id = stockEntryId });
        return exists == 1;
    }
}