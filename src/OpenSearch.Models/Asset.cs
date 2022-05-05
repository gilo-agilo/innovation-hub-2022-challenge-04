namespace OpenSearch.Models;

public class Asset
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal[]? ImageVector { get; set; }
}