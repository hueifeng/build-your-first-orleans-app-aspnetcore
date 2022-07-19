using Orleans;

namespace Sample
{
    public interface IUrlShortenerGrain : IGrainWithStringKey
    {
        Task SetUrl(string code);
        Task<string> GetUrl();
    }
}
