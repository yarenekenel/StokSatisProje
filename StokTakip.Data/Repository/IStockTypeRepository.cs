using StokTakip.Core.Entity;

namespace StokTakip.Data.Repository;

public interface IStockTypeRepository
{
    Task<IReadOnlyList<StockType>> GetAllAsync();
    Task<StockType?> GetByIdAsync(int id);
    Task<int> InsertAsync(StockType stockType);
    Task<bool> UpdateAsync(StockType stockType);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
}