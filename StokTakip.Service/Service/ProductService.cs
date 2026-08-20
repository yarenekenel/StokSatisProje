using StokTakip.Core.Constants;
using StokTakip.Core.Dto;
using StokTakip.Core.Entity;
using StokTakip.Core.Result;
using StokTakip.Data.Repository;
using StokTakip.Service.Interface;

namespace StokTakip.Service.Service;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<ProductListDto>>> GetListAsync(bool? isActive = null)
    {
        var products = await _repository.GetListAsync(isActive);
        return Result.Success(products);
    }

    public async Task<Result<Product>> GetByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null)
            return Result.Failure<Product>(Error.NotFound("Ürün bulunamadı."));

        return Result.Success(product);
    }

    public async Task<Result<IReadOnlyList<ProductLookupDto>>> GetLookupAsync()
    {
        var products = await _repository.GetLookupAsync();
        return Result.Success(products);
    }

    public async Task<Result<int>> CreateAsync(
        string name, int productTypeId, int unitId, int stockTypeId,
        string? accountingStockCode, decimal? criticalStockLevel, string? description)
    {
        name = (name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<int>(Error.Validation("Ürün adı boş olamaz."));

        if (await _repository.ExistsByNameAsync(name))
            return Result.Failure<int>(Error.Conflict("Aynı ürün iki kez oluşturulamaz."));

        var code = await GenerateNextCodeAsync();

        var product = new Product
        {
            Code = code,
            Name = name,
            ProductTypeId = productTypeId,
            UnitId = unitId,
            StockTypeId = stockTypeId,
            AccountingStockCode = string.IsNullOrWhiteSpace(accountingStockCode) ? null : accountingStockCode.Trim(),
            CriticalStockLevel = criticalStockLevel,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            IsActive = true
        };

        product.Id = await _repository.InsertAsync(product);
        return Result.Success(product.Id);
    }

    public async Task<Result> UpdateAsync(
        int id, string name, int productTypeId, int unitId, int stockTypeId,
        string? accountingStockCode, decimal? criticalStockLevel, string? description)
    {
        name = (name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("Ürün adı boş olamaz."));

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure(Error.NotFound("Ürün bulunamadı."));

        if (await _repository.ExistsByNameAsync(name, excludeId: id))
            return Result.Failure(Error.Conflict("Aynı ürün iki kez oluşturulamaz."));

        existing.Name = name;
        existing.ProductTypeId = productTypeId;
        existing.UnitId = unitId;
        existing.StockTypeId = stockTypeId;
        existing.AccountingStockCode = string.IsNullOrWhiteSpace(accountingStockCode) ? null : accountingStockCode.Trim();
        existing.CriticalStockLevel = criticalStockLevel;
        existing.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        await _repository.UpdateAsync(existing);
        return Result.Success();
    }

    public async Task<Result> SetActiveAsync(int id, bool isActive)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure(Error.NotFound("Ürün bulunamadı."));

        if (!isActive && await _repository.HasStockMovementsAsync(id))
            return Result.Failure(Error.Conflict("Stok hareketi olan bir ürün pasif hale getirilemez."));

        await _repository.SetActiveAsync(id, isActive);
        return Result.Success();
    }

    private async Task<string> GenerateNextCodeAsync()
    {
        var maxSequence = await _repository.GetMaxCodeSequenceAsync();
        return ProductCodeConstants.Format(maxSequence + 1);
    }
}