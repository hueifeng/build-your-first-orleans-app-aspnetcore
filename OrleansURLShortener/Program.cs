using System.Net;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using OrleansURLShortener;

var builder = WebApplication.CreateBuilder();

builder.Host.UseOrleans(siloBuilder =>
{
    //siloBuilder.UseLocalhostClustering();
    //siloBuilder.AddMemoryGrainStorage("urls");
    siloBuilder
        .UseLocalhostClustering()
        //.UseAdoNetClustering(options =>
        //{
        //    options.Invariant = "Npgsql";
        //    options.ConnectionString =
        //        "Server=localhost;Port=5432;User Id=postgres;Password=mysecretpassword;Database=test.db";
        //})
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "dev";
            options.ServiceId = "OrleansBasics";
        })
        .Configure<EndpointOptions>(options =>
        {
            options.AdvertisedIPAddress = IPAddress.Loopback;
        })
        .ConfigureApplicationParts(x => x.AddApplicationPart(Assembly.GetEntryAssembly()))
        .AddAdoNetGrainStorage("urls", options =>
        {
            options.Invariant = "Npgsql";
            options.ConnectionString =
                "Server=localhost;Port=5432;User Id=postgres;Password=mysecretpassword;Database=test.db";
        });

});

var app = builder.Build();

var grainFactory = app.Services.GetRequiredService<IGrainFactory>();

app.MapGet("/", () => "Hello World!");

app.MapGet("/shorten/{*path}", async (HttpContext context, string path) =>
{
    // var shortenedRouteSegment = Guid.NewGuid().GetHashCode().ToString("X");
    var shortenedRouteSegment = ShortUrlGenerator.MurmurHash3(path).ToString();
    var shortenerGrain = grainFactory.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);
    await shortenerGrain.SetUrl(shortenedRouteSegment, path);
    var resultBuilder = new UriBuilder(context.Request.GetEncodedUrl())
    {
        Path = $"/go/{shortenedRouteSegment}"
    };

    return Results.Ok(resultBuilder.Uri);
});

app.MapGet("/go/{shortenedRouteSegment}", async (string shortenedRouteSegment) =>
{
    var shortenerGrain = grainFactory.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);
    var url = await shortenerGrain.GetUrl();

    return url is not null ? Results.Redirect(url) : Results.NotFound();
});

app.Run();

public class UrlDictionary
{
    public UrlDictionary() { }
    public UrlDictionary(string code, string hashval)
    {
        Code = code;
        HashVal = hashval;
    }
    public string Code { get; set; }

    public string HashVal { get; set; }

    public string Url { get; set; }

    public DateTime InsertAt { get; set; }
}

public class UrlShortenerGrain : Grain, IUrlShortenerGrain
{
    private IPersistentState<UrlDictionary> _cache;

    public UrlShortenerGrain(
        [PersistentState(
            stateName: "url",
            storageName: "urls")]
            IPersistentState<UrlDictionary> state)
    {
        _cache = state;
    }

    public async Task SetUrl(string shortenedRouteSegment, string fullUrl)
    {
        _cache.State = new UrlDictionary(shortenedRouteSegment, fullUrl);
        await _cache.WriteStateAsync();
    }

    public Task<string> GetUrl()
    {
        return Task.FromResult(_cache.State.HashVal);
    }
}

public interface IUrlShortenerGrain : IGrainWithStringKey
{
    Task SetUrl(string shortenedRouteSegment, string fullUrl);
    Task<string> GetUrl();
}