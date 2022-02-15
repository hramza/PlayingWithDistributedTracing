using SearchEngine.Shared.Abstractions;

namespace SearchEngine.Shared.Infrastructure.Http
{
    public class CurrencyApiProvider : ApiClientBase, ICurrencyApiProvider
    {
        public CurrencyApiProvider(HttpClient httpClient) : base(httpClient) { }

        public async ValueTask<CurrencyData> GetCurrencyDataAsync(CurrencyRequest request)
        {
            var response = await GetHttp<CurrencyData>($"api/ticker/{request.From}/{request.To}");

            return response!.Data;
        }
    }
}
