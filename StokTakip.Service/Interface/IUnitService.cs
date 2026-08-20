using StokTakip.Core.Entity;
using StokTakip.Core.Result;

namespace StokTakip.Service.Interface;

public interface IUnitService
{
    Task<Result<IReadOnlyList<Unit>>> GetAllAsync();
    Task<Result<Unit>> GetByIdAsync(int id);
    Task<Result<Unit>> CreateAsync(string name);
    Task<Result<Unit>> UpdateAsync(int id, string name);
    Task<Result> DeleteAsync(int id);
}