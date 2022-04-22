using Elasticsearch.Net;
using Nest;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var pool = new SingleNodeConnectionPool(new Uri("https://search-testdomain-6ymb6zjdmjqxog7kln72dpya7m.eu-west-1.es.amazonaws.com"));
var settings = new ConnectionSettings(pool)
    .BasicAuthentication("testdomainUser", "Qwerty1234!")
    .DefaultIndex("books");
var client = new ElasticClient(settings);
builder.Services.AddSingleton(client);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();