using StokTakip.Core.Entity;

namespace StokTakip.Data.Repository;

public interface ICompanyRepository
{
    Task<IReadOnlyList<Company>> GetAllAsync(bool? isActive = null);
    Task<Company?> GetByIdAsync(int id);
    Task<int> InsertAsync(Company company);
    Task<bool> UpdateAsync(Company company);
    Task<bool> SetActiveAsync(int id, bool isActive);
    Task<bool> ExistsByTaxNumberAsync(string taxNumber, int? excludeId = null);
}