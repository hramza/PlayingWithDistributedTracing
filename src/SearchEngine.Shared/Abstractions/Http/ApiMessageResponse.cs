using SearchEngine.Shared.Serializers;

namespace SearchEngine.Shared.Abstractions.Http
{
    public class ApiMessageResponse
    {
        public HttpResponseMessage Message { get; }

        public string ResponseBody { get; }

        public List<ApiError> Errors { get; protected set; }

        public ApiMessageResponse(HttpResponseMessage httpResponseMessage, string responseBody)
        {
            Message = httpResponseMessage;
            ResponseBody = responseBody;
            Errors = new List<ApiError>();
        }

        public ApiResponse ToApiResponse() => new ApiResponse(Errors);

        public static async Task<ApiMessageResponse> FromMessage(HttpResponseMessage message)
        {
            if (message is null) return default!;

            var body = await message.Content.ReadAsStringAsync();

            var response = new ApiMessageResponse(message, body);
            if (!message.IsSuccessStatusCode)
                response.HandleErrors();

            return response;
        }

        protected void HandleErrors()
        {
            var errorMessage = !string.IsNullOrEmpty(ResponseBody) ? ResponseBody : Message.StatusCode.ToString();

            try
            {
                Errors = JsonHelper.Deserialize<List<ApiError>>(ResponseBody) ?? new List<ApiError>();
                if (Errors.Count == 0)
                {
                    Errors.Add(new ApiError(errorMessage));
                }
            }
            catch
            {
                Errors.Add(new ApiError(errorMessage));
            }
        }
    }
}
