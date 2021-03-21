using System.Threading.Tasks;
using Thoth.Models;

namespace Thoth
{
    public interface IGateway
    {
        Task<Response> SendRequest(Request req, Cache cache);
    }
}