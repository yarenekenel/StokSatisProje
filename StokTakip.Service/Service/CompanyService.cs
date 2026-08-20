using StokTakip.Core.Entity;
using StokTakip.Core.Result;
using StokTakip.Data.Repository;
using StokTakip.Service.Interface;

namespace StokTakip.Service.Service;

public sealed class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _repository;

    public CompanyService(ICompanyRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<Company>>> GetAllAsync(bool? isActive = null)
    {
        var companies = await _repository.GetAllAsync(isActive);
        return Result.Success(companies);
    }

    public async Task<Result<Company>> GetByIdAsync(int id)
    {
        var company = await _repository.GetByIdAsync(id);
        if (company is null)
            return Result.Failure<Company>(Error.NotFound("Firma bulunamadı."));

        return Result.Success(company);
    }

    public async Task<Result<Company>> CreateAsync(string companyName, string? taxNumber, string? phone, string? email, string? address)
    {
        companyName = (companyName ?? string.Empty).Trim();
        taxNumber = string.IsNullOrWhiteSpace(taxNumber) ? null : taxNumber.Trim();

        if (string.IsNullOrWhiteSpace(companyName))
            return Result.Failure<Company>(Error.Validation("Firma adı boş olamaz."));

        if (taxNumber is not null && await _repository.ExistsByTaxNumberAsync(taxNumber))
            return Result.Failure<Company>(Error.Conflict("Bu vergi numarasına sahip bir firma zaten mevcut."));

        var company = new Company
        {
            CompanyName = companyName,
            TaxNumber = taxNumber,
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim(),
            IsActive = true
        };
        company.Id = await _repository.InsertAsync(company);

        return Result.Success(company);
    }

    public async Task<Result<Company>> UpdateAsync(int id, string companyName, string? taxNumber, string? phone, string? email, string? address)
    {
        companyName = (companyName ?? string.Empty).Trim();
        taxNumber = string.IsNullOrWhiteSpace(taxNumber) ? null : taxNumber.Trim();

        if (string.IsNullOrWhiteSpace(companyName))
            return Result.Failure<Company>(Error.Validation("Firma adı boş olamaz."));

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure<Company>(Error.NotFound("Firma bulunamadı."));

        if (taxNumber is not null && await _repository.ExistsByTaxNumberAsync(taxNumber, excludeId: id))
            return Result.Failure<Company>(Error.Conflict("Bu vergi numarasına sahip bir firma zaten mevcut."));

        existing.CompanyName = companyName;
        existing.TaxNumber = taxNumber;
        existing.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        existing.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        existing.Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();

        await _repository.UpdateAsync(existing);

        return Result.Success(existing);
    }

    public async Task<Result> SetActiveAsync(int id, bool isActive)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure(Error.NotFound("Firma bulunamadı."));

        await _repository.SetActiveAsync(id, isActive);
        return Result.Success();
    }
}