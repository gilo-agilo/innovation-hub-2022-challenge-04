using Elasticsearch.Net;
using Nest;

namespace OpenSearchServerless;

public class ElasticSearchClient
{
    public static ElasticClient Instance { get; }

    static ElasticSearchClient()
    {
        var pool = new SingleNodeConnectionPool(new Uri("https://search-testdomain-6ymb6zjdmjqxog7kln72dpya7m.eu-west-1.es.amazonaws.com"));
        var settings = new ConnectionSettings(pool)
            .BasicAuthentication("testdomainUser", "Qwerty1234!")
            .DefaultIndex("books");
        Instance = new ElasticClient(settings);
    }
}