using SearchEngine.Shared.Abstractions.Http;
using System.Text;

namespace SearchEngine.Shared.Infrastructure.Http
{
    public abstract class ApiClientBase
    {
        protected readonly HttpClient _httpClient;

        public ApiClientBase(HttpClient httpClient) => _httpClient = httpClient;

        private string BaseAddress => _httpClient.BaseAddress!.ToString();

        protected Task<ApiMessageResponse<T>> GetHttp<T>(string path)
        {
            return SendHttp<T>(() => new HttpRequestMessage(HttpMethod.Get, new Uri(BaseAddress + path)));
        }

        private async Task<ApiMessageResponse<T>> SendHttp<T>(Func<HttpRequestMessage> requestMessageFunc)
        {
            try
            {
                var requestMessage = requestMessageFunc();

                var response = await _httpClient.SendAsync(requestMessage);
                return await ApiMessageResponse<T>.FromMessage(response);
            }
            catch (Exception ex)
            {
                return new ApiMessageResponse<T>(new List<ApiError> { new ApiError(GetErrorMessage(ex)) });
            }
        }

        private string GetErrorMessage(Exception ex, string delimeter = " --- ")
        {
            Exception? current = ex;
            StringBuilder builder = new();

            while (current is not null)
            {
                if (builder.Length > 0)
                    builder.Append(delimeter);

                builder.Append(current.Message);

                current = current.InnerException;
            }

            return builder.ToString();
        }
    }
}
