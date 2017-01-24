using System.Net.Http;
using System.Threading.Tasks;

namespace Implementation
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> PostStringAsync(string requestUri, string content);
    }

    public class HttpClient : IHttpClient
    {
        private readonly System.Net.Http.HttpClient _client;

        public HttpClient()
        {
            _client = new System.Net.Http.HttpClient();
        }

        public Task<HttpResponseMessage> PostStringAsync(string requestUri, string content)
        {
            return _client.PostAsync(requestUri, new StringContent(content));
        }
    }
}