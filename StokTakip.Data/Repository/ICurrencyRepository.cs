using StokTakip.Core.Entity;

namespace StokTakip.Data.Repository;

public interface ICurrencyRepository
{
    Task<IReadOnlyList<Currency>> GetAllAsync();
    Task<Currency?> GetByIdAsync(int id);
    Task<int> InsertAsync(Currency currency);
    Task<bool> UpdateAsync(Currency currency);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsByCodeAsync(string code, int? excludeId = null);
}