using StokTakip.Core.Dto;
using StokTakip.Core.Entity;
using StokTakip.Core.Result;

namespace StokTakip.Service.Interface;

public interface IStockEntryService
{
    Task<Result<IReadOnlyList<StockEntryListDto>>> GetListAsync();
    Task<Result<StockEntry>> GetByIdAsync(long id);
    Task<Result<long>> CreateAsync(int productId, int? companyId, string? invoiceNo, string? goodsReceiptNo, string? receivedBy, decimal quantity, decimal unitPrice, int currencyId, decimal exchangeRate, DateTime date, string? description);
    Task<Result> UpdateAsync(long id, int productId, int? companyId, string? invoiceNo, string? goodsReceiptNo, string? receivedBy, decimal quantity, decimal unitPrice, int currencyId, decimal exchangeRate, DateTime date, string? description);
    Task<Result> DeleteAsync(long id);
}