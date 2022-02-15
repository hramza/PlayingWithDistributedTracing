namespace SearchEngine.Shared.Abstractions.Http
{
    public class ApiResponse
    {
        public ApiResponse(List<ApiError> errors) => Errors = errors;

        public bool Success => Errors is null || Errors.Count == 0;

        public List<ApiError> Errors { get; protected set; }
    }
}
