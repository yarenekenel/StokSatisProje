using StokTakip.Core.Dto;
using StokTakip.Core.Entity;

namespace StokTakip.Data.Repository;

public interface IStockEntryRepository
{
    Task<StockEntry?> GetByIdAsync(long id);
    Task<IReadOnlyList<StockEntry>> GetAllAsync();
    Task<IReadOnlyList<StockEntryListDto>> GetListAsync();
    Task<IReadOnlyList<StockEntry>> GetByProductIdAsync(int productId);
    Task<IReadOnlyList<StockEntryOpenLotDto>> GetOpenLotsByProductIdAsync(int productId);
    Task<StockEntryOpenLotDto?> GetOpenLotByIdAsync(long stockEntryId);
    Task<IReadOnlyList<StockLotLookupDto>> GetOpenLotsLookupAsync();
    Task<long> InsertAsync(StockEntry stockEntry);
    Task<bool> UpdateAsync(StockEntry stockEntry);
    Task<bool> DeleteAsync(long id);
    Task<bool> HasIssuesAsync(long stockEntryId);
}