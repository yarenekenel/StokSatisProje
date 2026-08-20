namespace StokTakip.Core.Dto;

public sealed class ProjectCostDto
{
    public int ProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public decimal? OfferAmountTry { get; set; }
    public decimal TotalIssueCostTry { get; set; }
    public decimal? EstimatedProfitTry { get; set; }
}

public sealed class ProjectCostDetailDto
{
    public int ProjectMachineId { get; set; }
    public string MachineCode { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public long? StockIssueId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public string? UnitName { get; set; }
    public string? GoodsReceiptNo { get; set; }
    public string? InvoiceNo { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? AmountTry { get; set; }
    public DateTime? Date { get; set; }
    public string? Description { get; set; }
}