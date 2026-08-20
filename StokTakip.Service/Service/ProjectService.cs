using StokTakip.Core.Dto;
using StokTakip.Core.Entity;
using StokTakip.Core.Result;
using StokTakip.Data.Repository;
using StokTakip.Service.Interface;

namespace StokTakip.Service.Service;

public sealed class ProjectService : IProjectService
{
    private readonly IProjectRepository _repository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICurrencyRepository _currencyRepository;

    public ProjectService(
        IProjectRepository repository,
        ICompanyRepository companyRepository,
        ICurrencyRepository currencyRepository)
    {
        _repository = repository;
        _companyRepository = companyRepository;
        _currencyRepository = currencyRepository;
    }

    public async Task<Result<IReadOnlyList<ProjectListDto>>> GetListAsync(bool? isActive = null)
    {
        var projects = await _repository.GetListAsync(isActive);
        return Result.Success(projects);
    }

    public async Task<Result<Project>> GetByIdAsync(int id)
    {
        var project = await _repository.GetByIdAsync(id);
        if (project is null)
            return Result.Failure<Project>(Error.NotFound("Proje bulunamadı."));

        return Result.Success(project);
    }

    public async Task<Result<int>> CreateAsync(
        string projectCode, int companyId, decimal? offerAmount,
        int? currencyId, decimal? exchangeRate, DateTime? startDate, string? description)
    {
        projectCode = (projectCode ?? string.Empty).Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(projectCode))
            return Result.Failure<int>(Error.Validation("Proje kodu boş olamaz."));

        var businessCheck = await ValidateBusinessRulesAsync(companyId, currencyId);
        if (!businessCheck.IsSuccess)
            return Result.Failure<int>(businessCheck.Error!);

        exchangeRate = await ApplyCurrencyDefaultAsync(currencyId, exchangeRate);

        if (await _repository.ExistsByCodeAsync(projectCode))
            return Result.Failure<int>(Error.Conflict("Bu proje kodu zaten kayıtlı."));

        var project = new Project
        {
            ProjectCode = projectCode,
            CompanyId = companyId,
            OfferAmount = offerAmount,
            CurrencyId = currencyId,
            ExchangeRate = exchangeRate,
            StartDate = startDate,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            IsActive = true
        };

        project.Id = await _repository.InsertAsync(project);
        return Result.Success(project.Id);
    }

    public async Task<Result> UpdateAsync(
        int id, string projectCode, int companyId, decimal? offerAmount,
        int? currencyId, decimal? exchangeRate, DateTime? startDate, string? description)
    {
        projectCode = (projectCode ?? string.Empty).Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(projectCode))
            return Result.Failure(Error.Validation("Proje kodu boş olamaz."));

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure(Error.NotFound("Proje bulunamadı."));

        var businessCheck = await ValidateBusinessRulesAsync(companyId, currencyId);
        if (!businessCheck.IsSuccess)
            return Result.Failure(businessCheck.Error!);

        exchangeRate = await ApplyCurrencyDefaultAsync(currencyId, exchangeRate);

        if (await _repository.ExistsByCodeAsync(projectCode, excludeId: id))
            return Result.Failure(Error.Conflict("Bu proje kodu zaten kayıtlı."));

        existing.ProjectCode = projectCode;
        existing.CompanyId = companyId;
        existing.OfferAmount = offerAmount;
        existing.CurrencyId = currencyId;
        existing.ExchangeRate = exchangeRate;
        existing.StartDate = startDate;
        existing.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        await _repository.UpdateAsync(existing);
        return Result.Success();
    }

    public async Task<Result> SetActiveAsync(int id, bool isActive)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure(Error.NotFound("Proje bulunamadı."));

        if (!isActive && await _repository.HasStockIssuesAsync(id))
            return Result.Failure(Error.Conflict("Bu projeye bağlı stok çıkışı var, pasife alınamaz."));

        await _repository.SetActiveAsync(id, isActive);
        return Result.Success();
    }

    private async Task<Result> ValidateBusinessRulesAsync(int companyId, int? currencyId)
    {
        var company = await _companyRepository.GetByIdAsync(companyId);
        if (company is null)
            return Result.Failure(Error.NotFound("Firma bulunamadı."));

        if (!company.IsActive)
            return Result.Failure(Error.Failure("Pasif firmaya proje oluşturulamaz."));

        if (currencyId.HasValue)
        {
            var currency = await _currencyRepository.GetByIdAsync(currencyId.Value);
            if (currency is null)
                return Result.Failure(Error.NotFound("Para birimi bulunamadı."));
        }

        return Result.Success();
    }

    private async Task<decimal?> ApplyCurrencyDefaultAsync(int? currencyId, decimal? exchangeRate)
    {
        if (!currencyId.HasValue)
            return exchangeRate;

        var currency = await _currencyRepository.GetByIdAsync(currencyId.Value);
        if (currency is not null && string.Equals(currency.Code, "TRY", StringComparison.OrdinalIgnoreCase))
            return 1m;

        return exchangeRate;
    }
}