using StokTakip.Core.Entity;
using StokTakip.Core.Result;

namespace StokTakip.Service.Interface;

public interface IProductTypeService
{
    Task<Result<IReadOnlyList<ProductType>>> GetAllAsync();
    Task<Result<ProductType>> GetByIdAsync(int id);
    Task<Result<ProductType>> CreateAsync(string name, string codePrefix);
    Task<Result<ProductType>> UpdateAsync(int id, string name, string codePrefix);
    Task<Result> DeleteAsync(int id);
}