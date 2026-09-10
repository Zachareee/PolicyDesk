namespace PolicyDesk.Api.Models.Dtos;

public class PolicyListRequest
{
    public string? Search { get; set; }
    public PolicyStatus? Status { get; set; }
    public string? PolicyType { get; set; }
    public string? CustomerIdentifier { get; set; }
    public string? Insurer { get; set; }
    public decimal? MinPremium { get; set; }
    public decimal? MaxPremium { get; set; }
    public string SortBy { get; set; } = "UpdatedAt";
    public string SortDirection { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
