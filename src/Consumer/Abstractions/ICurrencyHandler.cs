namespace Consumer.Abstractions
{
    public interface ICurrencyHandler
    {
        Task HandleAsync(CurrencyRequest request, CancellationToken cancellationToken);
    }
}
