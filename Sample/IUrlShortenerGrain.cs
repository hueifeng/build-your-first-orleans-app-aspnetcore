using Orleans;

namespace Sample
{
    public interface IUrlShortenerGrain : IGrainWithStringKey
    {
        Task<string> GetUrl();
        Task SetUrl(string code);
        Task<bool> IsExists(string code);
    }
}
