using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace Thoth.Services
{
    public class Client : IClient
    {
        public const string Insecure = "Insecure";
        public const string Secure = "Secure";

        public readonly HttpClient client;

        public Client(IHttpClientFactory factory, IConfiguration configuration)
        {
            if (bool.Parse(configuration["Configuration:Insecure"]))
                client = factory.CreateClient(Insecure);
            else
                client = factory.CreateClient(Secure);
        }

        public HttpClient Get()
        {
            return client;
        }
    }
}