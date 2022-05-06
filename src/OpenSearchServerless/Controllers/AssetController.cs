using Microsoft.AspNetCore.Mvc;
using Nest;
using OpenSearch.Models;

namespace OpenSearchServerless.Controllers;

[ApiController]
[Route("[controller]")]
public class AssetController : ControllerBase
{
    private readonly ElasticClient _elasticClient;

    public AssetController(ElasticClient elasticClient)
    {
        _elasticClient = elasticClient;
    }

    // GET
    [HttpGet(Name = "GetAssets")]
    public IActionResult Get()
    {
        var assets = _elasticClient.Search<Asset>(
            x => x.Query(q => q.MatchAll()));

        if (assets.IsValid)
            return Ok(assets.Documents);

        return BadRequest(assets.DebugInformation);
    }

    [HttpPost(Name = "Search")]
    public IActionResult Search([FromBody] string term)
    {
        var result = _elasticClient.Search<Asset>(
            x => x.Query(q =>
                q.QueryString(y => y.Query(term).Fields(fs => fs.Fields(f => f.Title)))));

        if (result.IsValid)
            return Ok(result.Documents);

        return BadRequest(result.DebugInformation);
    }
}