namespace SearchEngine.Shared.Abstractions.Http
{
    public class ApiError
    {
        public ApiError(string errorMessage) => ErrorMessage = errorMessage;

        public string ErrorMessage { get; }
    }
}
