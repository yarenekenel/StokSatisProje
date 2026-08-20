using StokTakip.Core.Entity;

namespace StokTakip.Data.Repository;

public interface IUnitRepository
{
    Task<IReadOnlyList<Unit>> GetAllAsync();
    Task<Unit?> GetByIdAsync(int id);
    Task<int> InsertAsync(Unit unit);
    Task<bool> UpdateAsync(Unit unit);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
}