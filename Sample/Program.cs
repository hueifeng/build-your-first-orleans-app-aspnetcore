using Orleans;
using Orleans.Hosting;
 
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.UseLocalhostClustering();
    siloBuilder.AddMemoryGrainStorage("urls");

});
var app = builder.Build();



app.Run();

