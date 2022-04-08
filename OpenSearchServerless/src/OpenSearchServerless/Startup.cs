using Elasticsearch.Net;
using Nest;

namespace OpenSearchServerless;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        
        var pool = new SingleNodeConnectionPool(new Uri("https://search-testdomain-6ymb6zjdmjqxog7kln72dpya7m.eu-west-1.es.amazonaws.com"));
        var settings = new ConnectionSettings(pool)
            .BasicAuthentication("testdomainUser", "Qwerty1234!")
            .DefaultIndex("books");
        var client = new ElasticClient(settings);
        services.AddSingleton(client);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/",
                async context =>
                {
                    await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
                });
        });
    }
}