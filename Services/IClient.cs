using System.Net.Http;

namespace Thoth.Services
{
    public interface IClient
    {
        HttpClient Get();
    }
}