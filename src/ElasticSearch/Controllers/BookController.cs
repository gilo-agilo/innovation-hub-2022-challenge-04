using ElasticSearch.Model;
using Microsoft.AspNetCore.Mvc;
using Nest;

namespace ElasticSearch.Controllers;

[ApiController]
[Route("[controller]")]
public class BookController : ControllerBase
{
    private readonly ElasticClient _elasticClient;

    public BookController(ElasticClient elasticClient)
    {
        _elasticClient = elasticClient;
    }
    
    // GET
    [HttpGet(Name = "GetBooks")]
    public IActionResult Get()
    {
        var books = _elasticClient.Search<Book>(
            x => x.Query(q => q.MatchAll()));

        if (books.IsValid)
            return Ok(books.Documents);

        return BadRequest(books.DebugInformation);
    }

    [HttpPost(Name = "Search")]
    public IActionResult Search([FromBody] string term)
    {
        var res = _elasticClient.Search<Book>(
            x => x.Query(q =>
                q.QueryString(y => y.Query(term).Fields(fs => fs.Fields(f => f.Title)))));

        if (res.IsValid)
            return Ok(res.Documents);

        return BadRequest("dfgdzss");
    }
}