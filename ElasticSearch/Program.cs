using Elasticsearch.Net;
using Nest;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure elastic
var pool = new SingleNodeConnectionPool(new Uri($"http://elasticsearch:9200"));
var settings = new ConnectionSettings(pool)
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