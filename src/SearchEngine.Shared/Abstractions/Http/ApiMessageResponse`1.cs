using SearchEngine.Shared.Serializers;

namespace SearchEngine.Shared.Abstractions.Http
{
    public class ApiMessageResponse<T> : ApiMessageResponse
    {
        public ApiMessageResponse(HttpResponseMessage response, string body)
            : base(response, body)
        {
            Data = default!;
        }

        public ApiMessageResponse(List<ApiError> errors) : base(default!, default!)
        {
            Errors = errors;
            Data = default!;
        }

        public new ApiResponse<T> ToApiResponse() => new ApiResponse<T>(Data, Errors);

        public T Data { get; private set; }

        public new static async Task<ApiMessageResponse<T>> FromMessage(HttpResponseMessage message)
        {
            if (message is null) return default!;

            var body = await message.Content.ReadAsStringAsync();

            var response = new ApiMessageResponse<T>(message, body);

            if (message.IsSuccessStatusCode)
            {
                response.Data = JsonHelper.Deserialize<T>(response.ResponseBody)!;
            }
            else
            {
                response.HandleErrors();
            }

            return response;
        }
    }
}
