namespace SearchEngine.Shared.Abstractions
{
    public interface ICurrencyApiProvider
    {
        ValueTask<CurrencyData> GetCurrencyDataAsync(CurrencyRequest request);
    }
}
