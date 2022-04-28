namespace OpenSearch.Models;

public class Asset
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public Uri FileUrl { get; set; }
    public int[] Features { get; set; }
    public Guid[] Users { get; set; }
    public Guid[] UserGroups { get; set; }
    public Guid[] BusinessUnits { get; set; }
}