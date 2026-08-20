using StokTakip.Core.Dto;
using StokTakip.Core.Entity;
using StokTakip.Core.Result;
using StokTakip.Data.Repository;
using StokTakip.Service.Interface;

namespace StokTakip.Service.Service;

public sealed class StockEntryService : IStockEntryService
{
    private readonly IStockEntryRepository _repository;
    private readonly IProductRepository _productRepository;
    private readonly ICurrencyRepository _currencyRepository;

    public StockEntryService(
        IStockEntryRepository repository,
        IProductRepository productRepository,
        ICurrencyRepository currencyRepository)
    {
        _repository = repository;
        _productRepository = productRepository;
        _currencyRepository = currencyRepository;
    }

    public async Task<Result<IReadOnlyList<StockEntryListDto>>> GetListAsync()
    {
        var list = await _repository.GetListAsync();
        return Result.Success(list);
    }

    public async Task<Result<StockEntry>> GetByIdAsync(long id)
    {
        var entry = await _repository.GetByIdAsync(id);
        if (entry is null)
            return Result.Failure<StockEntry>(Error.NotFound("Stok girişi bulunamadı."));

        return Result.Success(entry);
    }

    public async Task<Result<long>> CreateAsync(
        int productId, int? companyId, string? invoiceNo, string? goodsReceiptNo, string? receivedBy,
        decimal quantity, decimal unitPrice, int currencyId, decimal exchangeRate, DateTime date, string? description)
    {
        var validation = await ValidateAsync(productId, currencyId, quantity, unitPrice, exchangeRate);
        if (!validation.IsSuccess)
            return Result.Failure<long>(validation.Error!);

        var entry = new StockEntry
        {
            ProductId = productId,
            CompanyId = companyId,
            InvoiceNo = string.IsNullOrWhiteSpace(invoiceNo) ? null : invoiceNo.Trim(),
            GoodsReceiptNo = string.IsNullOrWhiteSpace(goodsReceiptNo) ? null : goodsReceiptNo.Trim(),
            ReceivedBy = string.IsNullOrWhiteSpace(receivedBy) ? null : receivedBy.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice,
            CurrencyId = currencyId,
            ExchangeRate = exchangeRate,
            Date = date,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim()
        };

        var id = await _repository.InsertAsync(entry);
        return Result.Success(id);
    }

    public async Task<Result> UpdateAsync(
        long id, int productId, int? companyId, string? invoiceNo, string? goodsReceiptNo, string? receivedBy,
        decimal quantity, decimal unitPrice, int currencyId, decimal exchangeRate, DateTime date, string? description)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure(Error.NotFound("Stok girişi bulunamadı."));

        if (await _repository.HasIssuesAsync(id))
            return Result.Failure(Error.Conflict("Bu girişten stok çıkışı yapılmış, düzenlenemez."));

        var validation = await ValidateAsync(productId, currencyId, quantity, unitPrice, exchangeRate);
        if (!validation.IsSuccess)
            return Result.Failure(validation.Error!);

        existing.ProductId = productId;
        existing.CompanyId = companyId;
        existing.InvoiceNo = string.IsNullOrWhiteSpace(invoiceNo) ? null : invoiceNo.Trim();
        existing.GoodsReceiptNo = string.IsNullOrWhiteSpace(goodsReceiptNo) ? null : goodsReceiptNo.Trim();
        existing.ReceivedBy = string.IsNullOrWhiteSpace(receivedBy) ? null : receivedBy.Trim();
        existing.Quantity = quantity;
        existing.UnitPrice = unitPrice;
        existing.CurrencyId = currencyId;
        existing.ExchangeRate = exchangeRate;
        existing.Date = date;
        existing.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        await _repository.UpdateAsync(existing);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(long id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure(Error.NotFound("Stok girişi bulunamadı."));

        if (await _repository.HasIssuesAsync(id))
            return Result.Failure(Error.Conflict("Bu girişten stok çıkışı yapılmış, silinemez."));

        await _repository.DeleteAsync(id);
        return Result.Success();
    }

    private async Task<Result> ValidateAsync(int productId, int currencyId, decimal quantity, decimal unitPrice, decimal exchangeRate)
    {
        if (quantity <= 0)
            return Result.Failure(Error.Validation("Miktar sıfırdan büyük olmalıdır."));

        if (unitPrice < 0)
            return Result.Failure(Error.Validation("Birim fiyat negatif olamaz."));

        if (exchangeRate <= 0)
            return Result.Failure(Error.Validation("Kur sıfırdan büyük olmalıdır."));

        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null)
            return Result.Failure(Error.NotFound("Ürün bulunamadı."));

        var currency = await _currencyRepository.GetByIdAsync(currencyId);
        if (currency is null)
            return Result.Failure(Error.NotFound("Para birimi bulunamadı."));

        return Result.Success();
    }
}