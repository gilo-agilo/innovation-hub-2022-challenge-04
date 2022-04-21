namespace OpenSearchPerformanceTest;

    using Nest;
    using System.Diagnostics;
    using System;
using OpenSearchPerformanceTest.Models;

public class Program
    {
        public static int ChunkSize = 500;
        private static Random random = new Random();

        public static async Task Main()
        {
            var indexName = "assets";
            var total = 10000;

            var client = GetElasticClient(indexName);

            DropIndex(client, indexName);
            DropAssets(client);

            var assets = GenerateTestAssets(total);

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            var cnk = 3;

            ChunkAdd(indexName, client, assets, cnk);

            Console.WriteLine("Adding Time is {0} ms for {1}", stopwatch.ElapsedMilliseconds, total);

            var assetsUpdated = GetUpdatedAssets(assets);

            stopwatch.Restart();

            ChunkUpdate(indexName, client, cnk, assetsUpdated);

            Console.WriteLine("Updating Time is {0} ms for {1}", stopwatch.ElapsedMilliseconds, total);

            await CheckSwarchPerfomanse(indexName, total, client);

            Console.WriteLine();
        }

    private static async Task CheckSwarchPerfomanse(string indexName, int total, ElasticClient client)
    {
        long fullWord = 0;
        long partWord = 0;
        long metadataSearch = 0;
        int searchIteration = 100;
        for (int i = 0; i < searchIteration; i++)
        {
            Stopwatch searchTimer = new Stopwatch();

            searchTimer.Start();
            var num = random.Next(1, total);
            var str = $"*{num.ToString()}*";

            var scanResults = client.Search<Asset>(s => s
            .Explain()
            .From(0)
            .Size(10000)
            .Query(
                q => q.Match(
                    qs => qs.Field("displayname")

                     .Query(str)
                     .MinimumShouldMatch(100)
                    )
            ));
            fullWord += searchTimer.ElapsedMilliseconds;

            searchTimer.Restart();

            var searchResponse1 = await client
             .SearchAsync<Asset>(s => s
                .Index(indexName)
                .Size(5)
                .Query(q => q.Wildcard(
                    c => c.Field(p => p.DisplayName)
                    .Value(str)
                    ))
                );

            partWord += searchTimer.ElapsedMilliseconds;

            searchTimer.Restart();

            var searchResponse2 = await client
            .SearchAsync<Asset>(
                s => s
                    .Index(indexName)
                    .Size(1)
                    .PostFilter(pf => pf
                       .Bool(b => b.
                          Must(must => must
                         .MatchPhrase(m => m
                             .Field(f => f.MetadataTags)
                             .Query($"*{ToGuid(num * 10).ToString()}*"))))));

            metadataSearch += searchTimer.ElapsedMilliseconds;

            searchTimer.Restart();
        }

        Console.WriteLine($"searchtime fullWord {fullWord / searchIteration} ms for {total}");

        Console.WriteLine($"searchtime partWord {partWord / searchIteration} ms for {total}");

        Console.WriteLine($"searchtime metadata {metadataSearch / searchIteration} ms for {total}");
    }

    private static void ChunkUpdate(string indexName, ElasticClient client, int cnk, List<Asset> assetsUpdated)
    {
        for (int i = 0; i < assetsUpdated.Count / ChunkSize / cnk; i++)
        {
            if (i % 2 == 0)
            {
                GC.Collect();
            }

            BulkAssetUpdate(indexName, client, assetsUpdated.Skip(i * ChunkSize * cnk).Take(ChunkSize * cnk).ToList(), i * ChunkSize * cnk);
        }
    }

    private static void ChunkAdd(string indexName, ElasticClient client, List<Asset> assets, int cnk)
    {
        for (int i = 0; i < Math.Ceiling(assets.Count * 1.0 / ChunkSize / cnk); i++)
        {
            if (i % 2 == 0)
            {
                GC.Collect();
            }

            BulkAssetAdd(indexName, client, assets.Skip(i * ChunkSize * cnk).Take(ChunkSize * cnk).ToList(), i * ChunkSize * cnk);
        }
    }

    private static void DropAssets(ElasticClient client)
    {
        client.DeleteByQuery<Asset>(del => del
            .Query(q => q.QueryString(qs => qs.Query("*")))
        );
    }

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ ";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static void BulkAssetUpdate(string indexName, ElasticClient client, List<Asset> assetsUpdated, int num)
    {
        var waitHandle = new CountdownEvent(1);
        var i = 0;
        var bulkAllObservable2 = client.BulkAll(assetsUpdated, b => b
            .Index(indexName)
            .BackOffTime("30s")
            .BackOffRetries(2)
            .RefreshOnCompleted()
            .MaxDegreeOfParallelism(Environment.ProcessorCount)
            .Size(ChunkSize)
        ).Subscribe(new BulkAllObserver(
                 onNext: x => Console.WriteLine($"Chunk Added {num + i++ * ChunkSize}"),
                 onCompleted: () => waitHandle.Signal()
            ));

        waitHandle.Wait();
    }

    private static void BulkAssetAdd(string indexName, ElasticClient client, List<Asset> assets, int num)
    {
        var i = 0;
        var waitHandle = new CountdownEvent(1);

        var bulkAll = client.BulkAll(assets, b => b
            .Index(indexName)
            .BackOffTime("30s")
            .BackOffRetries(2)
            .RefreshOnCompleted()
            .MaxDegreeOfParallelism(Environment.ProcessorCount)
            .Size(ChunkSize)
        ).Subscribe(new BulkAllObserver(
                 onNext: x => Console.WriteLine($"Chunk Added {num + i++ * ChunkSize}"),
                 onCompleted: () => waitHandle.Signal()
            ));

        waitHandle.Wait();
    }

    public static void DropIndex(ElasticClient client, string indexName)
    {
        if (client.Indices.Exists(indexName).Exists)
        {
            client.Indices.Delete(indexName);
        }
    }

    public static ElasticClient GetElasticClient(string indexName)
    {
        var settings = new ConnectionSettings(new Uri("https://"))
            .DefaultIndex(indexName);

        return new ElasticClient(settings);
    }

    public static List<Asset> GenerateTestAssets(int count)
    {
        var assets = new List<Asset>();

        for (int i = 0; i < count; i++)
        {
            assets.Add(
                new Asset
                {
                    Id = ToGuid(i),
                    DisplayName = $"DisplayName {i} {RandomString(random.Next(10, 200))}",
                    Name = $"Name {i} {RandomString(random.Next(10, 30))}",
                    CreatedById = ToGuid(i),
                    CreatedBy = "CreatedBy" + i,
                    CreatedDate = DateTime.Now,
                    Tags = new[] { ToGuid(i * 10), ToGuid(i * 100), ToGuid(i * 1000) },
                    FileId = ToGuid(i),
                    FileName = "ImageName" + i,
                    FileExtension = i % 2 == 0 ? ".jpg" : ".png",
                    BusinesUnits = new[] { ToGuid(i * 10), ToGuid(i * 100), ToGuid(i * 1000) },
                    MetadataTags = new[] { ToGuid(i * 10), ToGuid(i * 100), ToGuid(i * 1001), ToGuid(i * 1002) }
                });
        }
        return assets;
    }

    public static Guid ToGuid(int value)
    {
        byte[] bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        var ewqe = new Guid(bytes);
        return ewqe;
    }

    public static List<Asset> GetUpdatedAssets(List<Asset> assets)
    {
        var updated = new List<Asset>();
        var i = 1;
        foreach (var asset in assets)
        {
            updated.Add(
                new Asset
                {
                    Id = asset.Id,
                    DisplayName = asset.DisplayName + " " + "New",
                    Name = asset.Name + " " + "New",
                    CreatedById = asset.CreatedById,
                    CreatedBy = asset.CreatedBy,
                    CreatedDate = asset.CreatedDate,
                    Tags = new[] { ToGuid(i * 10), ToGuid(i * 100) },
                    FileId = asset.FileId,
                    FileName = asset.FileName,
                    FileExtension = asset.FileExtension,
                    BusinesUnits = new[] { ToGuid(i * 10), ToGuid(i * 100) },
                    MetadataTags = new[] { ToGuid(i * 10), ToGuid(i * 100) },
                    UpdatedBy = "Updater" + i,
                    UpdatedById = ToGuid(i * 10)
                });
            i++;
        }
        return updated;

    }


}







