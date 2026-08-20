using StokTakip.Core.Entity;

namespace StokTakip.Data.Repository;

public interface IProductTypeRepository
{
    Task<IReadOnlyList<ProductType>> GetAllAsync();
    Task<ProductType?> GetByIdAsync(int id);
    Task<int> InsertAsync(ProductType productType);
    Task<bool> UpdateAsync(ProductType productType);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<bool> ExistsByCodePrefixAsync(string codePrefix, int? excludeId = null);
}