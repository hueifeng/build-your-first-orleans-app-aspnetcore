using Microsoft.AspNetCore.Http.Extensions;
using Orleans;
using Orleans.Hosting;
using OrleansURLShortener;
using Sample;

var builder = WebApplication.CreateBuilder();

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.UseLocalhostClustering();
    siloBuilder.AddMemoryGrainStorage("urls");
});

var app = builder.Build();

var grainFactory = app.Services.GetRequiredService<IGrainFactory>();

app.MapGet("/", () => "Hello World!");

app.MapGet("/shorten/{*path}", async (HttpContext context, string path) =>
{
    var shortenedRouteSegment = ShortUrlGenerator.MurmurHash(path);
    var code = ShortUrlGenerator.Generator(shortenedRouteSegment);
    var shortenerGrain = grainFactory.GetGrain<IUrlShortenerGrain>(code);

    await shortenerGrain.SetUrl(code);
    var resultBuilder = new UriBuilder(context.Request.GetEncodedUrl())
    {
        Path = $"/go/{code}"
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

