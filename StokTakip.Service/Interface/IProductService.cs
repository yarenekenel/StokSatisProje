using StokTakip.Core.Dto;
using StokTakip.Core.Entity;
using StokTakip.Core.Result;

namespace StokTakip.Service.Interface;

public interface IProductService
{
    Task<Result<IReadOnlyList<ProductListDto>>> GetListAsync(bool? isActive = null);
    Task<Result<Product>> GetByIdAsync(int id);
    Task<Result<IReadOnlyList<ProductLookupDto>>> GetLookupAsync();
    Task<Result<int>> CreateAsync(string name, int productTypeId, int unitId, int stockTypeId, string? accountingStockCode, decimal? criticalStockLevel, string? description);
    Task<Result> UpdateAsync(int id, string name, int productTypeId, int unitId, int stockTypeId, string? accountingStockCode, decimal? criticalStockLevel, string? description);
    Task<Result> SetActiveAsync(int id, bool isActive);
}