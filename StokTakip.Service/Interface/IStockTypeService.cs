using StokTakip.Core.Entity;
using StokTakip.Core.Result;

namespace StokTakip.Service.Interface;

public interface IStockTypeService
{
    Task<Result<IReadOnlyList<StockType>>> GetAllAsync();
    Task<Result<StockType>> GetByIdAsync(int id);
    Task<Result<StockType>> CreateAsync(string name);
    Task<Result<StockType>> UpdateAsync(int id, string name);
    Task<Result> DeleteAsync(int id);
}