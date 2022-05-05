using Elasticsearch.Net;
using Nest;

namespace OpenSearchServerless.Services;

public class ElasticSearchClient
{
    public static ElasticClient Instance { get; }

    static ElasticSearchClient()
    {
        var pool = new SingleNodeConnectionPool(new Uri("https://search-testdomain-6ymb6zjdmjqxog7kln72dpya7m.eu-west-1.es.amazonaws.com"));
        var settings = new ConnectionSettings(pool)
            .BasicAuthentication("testdomainUser", "Qwerty1234!")
            .DefaultIndex("assets");
        Instance = new ElasticClient(settings);
    }
}