using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Thoth.Models;
using Thoth.Services;

namespace Thoth
{
    public class Gateway : IGateway
    {
        private readonly HttpClient client;
        private readonly ICachedResponseService cacheService;

        public Gateway(ICachedResponseService cacheService, IClient client)
        {
            this.cacheService = cacheService;
            this.client = client.Get();
        }

        public async Task<Response> SendRequest(Request req, Cache cache)
        {
            // Generate a unique key with the request
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req));
            return await cacheService.GetCachedResponse(System.Convert.ToBase64String(plainTextBytes).Trim(), cache, () => MakeRequest(req));
        }

        private async Task<Response> MakeRequest(Request req)
        {
            Response res;
            try
            {
                using (var newRequest = new HttpRequestMessage(new HttpMethod(req.Method), req.Endpoint))
                {
                    if (req.Authorization != string.Empty && req.Authorization != null)
                        newRequest.Headers.Add("Authorization", req.Authorization);

                    newRequest.Content = new StringContent(req.Body, Encoding.UTF8, req.ContentType);
                    using (var response = await client.SendAsync(newRequest))
                    {
                        return new Response
                        {
                            StatusCode = (int)response.StatusCode,
                            Body = await response.Content.ReadAsStringAsync()
                        };
                    }
                }
            }
            catch
            {
                return new Response
                {
                    StatusCode = 500,
                    Body = "{error: Server internal error}"
                };
            }
        }
    }
}