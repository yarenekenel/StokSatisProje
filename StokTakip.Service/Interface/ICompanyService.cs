using StokTakip.Core.Entity;
using StokTakip.Core.Result;

namespace StokTakip.Service.Interface;

public interface ICompanyService
{
    Task<Result<IReadOnlyList<Company>>> GetAllAsync(bool? isActive = null);
    Task<Result<Company>> GetByIdAsync(int id);
    Task<Result<Company>> CreateAsync(string companyName, string? taxNumber, string? phone, string? email, string? address);
    Task<Result<Company>> UpdateAsync(int id, string companyName, string? taxNumber, string? phone, string? email, string? address);
    Task<Result> SetActiveAsync(int id, bool isActive);
}