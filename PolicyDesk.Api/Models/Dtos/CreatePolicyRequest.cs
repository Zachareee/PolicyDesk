namespace PolicyDesk.Api.Models.Dtos;

public class CreatePolicyRequest
{
    public string PolicyNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerIdentifier { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty;
    public string Insurer { get; set; } = string.Empty;
    public decimal Premium { get; set; }
    public BillingFrequency BillingFrequency { get; set; } = BillingFrequency.Monthly;
    public decimal CoverageAmount { get; set; }
    public decimal Deductible { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string? AssignedAgent { get; set; }
    public string? ClaimReference { get; set; }
    public string? Notes { get; set; }
}
