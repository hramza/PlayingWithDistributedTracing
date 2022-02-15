namespace SearchEngine.Shared.Abstractions.Http
{
    public class ApiResponse<T> : ApiResponse
    {
        public ApiResponse(T data, List<ApiError> errors) : base(errors) => Data = data;

        public T Data { get; set; }
    }
}
