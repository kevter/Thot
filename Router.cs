using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Thoth.Models;
using Thoth.Utils;

namespace Thoth
{
    public class Router
    {
        public List<Route> Routes { get; set; }
        public Authentication AuthenticationService { get; set; }
        private readonly IGateway gateway;

        public Router(string routeConfigFilePath, IGateway gateway)
        {
            this.gateway = gateway;
            dynamic router = Json.LoadFromFile<dynamic>(routeConfigFilePath);

            Routes = Json.Deserialize<List<Route>>(Convert.ToString(router.routes));
            AuthenticationService = Json.Deserialize<Authentication>(Convert.ToString(router.authenticationService));
        }

        public async Task<Response> RouteRequest(HttpRequest request)
        {
            Request req = new Request();
            string requestPath = request.Path.ToString();

            Route route;
            try
            {
                route = Routes.First(r => requestPath.Contains(r.Endpoint, StringComparison.OrdinalIgnoreCase));

                if (!EvaluateHttpMethod(request.Method, route.Destination.HttpMethod))
                {
                    throw new Exception();
                }

                if (!EvaluatePaths(route.Endpoint, request.Path.ToString()))
                {
                    throw new Exception();
                }
            }
            catch
            {
                return ConstructErrorMessage("The path could not be found.");
            }

            req.Method = request.Method;
            req.ContentType = request.ContentType;
            req.Endpoint = route.Destination.Path + GeneratePathCall(request, route.Endpoint, requestPath);
            req.Body = ExtractBodyRequest(request);

            string authorization = request.Headers["Authorization"];
            if (authorization != null)
            {
                req.Authorization = authorization;
            }

            if (route.Destination.RequiresAuthentication)
            {
                Request reqAuth = new Request()
                {
                    Authorization = authorization,
                    Endpoint = AuthenticationService.Path,
                    Method = "POST"
                };

                string token = request.Headers["Authorization"];
                Response authResponse = await gateway.SendRequest(reqAuth, new Cache());
                if (authResponse.StatusCode != (int)HttpStatusCode.OK)
                {
                    return ConstructErrorMessage("Authentication failed.");
                }
            }

            return await gateway.SendRequest(req, route.Cache);
        }

        private Response ConstructErrorMessage(string error)
        {
            return new Response
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Body = error
            };
        }

        private bool EvaluatePaths(string endpointPath, string requestPath)
        {
            // Evalute the next case:
            // If request path and endpoint path are equals, example:
            // requestPath = /user/login  and endpointPath = /user/login
            if (endpointPath.Length == requestPath.Length)
            {
                return true;
            }

            // If start with endpoint rout
            if (!requestPath.StartsWith(endpointPath))
            {
                return false;
            }
            // If endpoint route contains request path but the substring
            // of request path and endpoint ptah are equals, example:
            // requestPath = /user/login/example  and endpointPath = /user/login
            // Error case:
            // requestPath = /user/loginexample  and endpointPath = /user/login
            if (requestPath.Substring(endpointPath.Length).StartsWith("/"))
            {
                return true;
            }

            // requestPath not found
            return false;
        }

        private bool EvaluateHttpMethod(string requestMethod, List<string> aceptedMethods)
        {
            if (aceptedMethods.Exists(m => m.Equals(requestMethod)))
            {
                return true;
            }

            return false;
        }

        private string GeneratePathCall(HttpRequest request, string endpointPath, string requestPath)
        {
            string queryString = request.QueryString.ToString();
            string path = requestPath.Substring(endpointPath.Length);

            return path + queryString;
        }

        private string ExtractBodyRequest(HttpRequest request)
        {
            string requestContent;
            using (Stream receiveStream = request.Body)
            {
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    requestContent = readStream.ReadToEnd();
                }
            }
            return requestContent;
        }
    }
}