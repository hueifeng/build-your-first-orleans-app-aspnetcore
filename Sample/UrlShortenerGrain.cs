using Orleans;
using Orleans.Runtime;

namespace Sample
{
    public class UrlShortenerGrain : Grain,IUrlShortenerGrain
    {
        private IPersistentState<UrlDictionary> _urlDictionaryState;
        public UrlShortenerGrain(
        [PersistentState(
            stateName: "url",
            storageName: "urls")]
            IPersistentState<UrlDictionary> state)
        {
            _urlDictionaryState = state;
        }

        public async Task<string> GetUrl()
        {
            return _urlDictionaryState.State.Url;
        }

        public async Task<bool> IsExists(string code)
        {
            return _urlDictionaryState.RecordExists;
        }

        public async Task SetUrl(string code)
        {
            _urlDictionaryState.State = new UrlDictionary
            {
                Code = code
            };
            await _urlDictionaryState.WriteStateAsync();
        }
    }
}
