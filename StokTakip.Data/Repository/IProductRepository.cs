using StokTakip.Core.Dto;
using StokTakip.Core.Entity;

namespace StokTakip.Data.Repository;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(bool? isActive = null);
    Task<Product?> GetByIdAsync(int id);
    Task<IReadOnlyList<ProductListDto>> GetListAsync(bool? isActive = null);
    Task<IReadOnlyList<ProductLookupDto>> GetLookupAsync();
    Task<int> InsertAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<bool> SetActiveAsync(int id, bool isActive);
    Task<bool> ExistsByCodeAsync(string code, int? excludeId = null);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<bool> HasStockMovementsAsync(int productId);
    Task<int> GetMaxCodeSequenceAsync();
}