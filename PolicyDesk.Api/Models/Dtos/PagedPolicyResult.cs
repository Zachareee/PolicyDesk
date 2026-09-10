namespace PolicyDesk.Api.Models.Dtos;

public class PagedPolicyResult
{
    public List<PolicyListItemDto> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
