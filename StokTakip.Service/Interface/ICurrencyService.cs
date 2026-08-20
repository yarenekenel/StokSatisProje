using StokTakip.Core.Entity;
using StokTakip.Core.Result;

namespace StokTakip.Service.Interface;

public interface ICurrencyService
{
    Task<Result<IReadOnlyList<Currency>>> GetAllAsync();
    Task<Result<Currency>> GetByIdAsync(int id);
    Task<Result<Currency>> CreateAsync(string code, string name);
    Task<Result<Currency>> UpdateAsync(int id, string code, string name);
    Task<Result> DeleteAsync(int id);
}