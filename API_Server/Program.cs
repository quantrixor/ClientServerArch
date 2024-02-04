using ModelDataBase.Model;
using Newtonsoft.Json;
using System;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace API_Server
{
    internal class Program
    {
        static HttpListener listener;

        private static async Task RouteRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            var path = request.Url.AbsolutePath;
            var method = request.HttpMethod;

            if (path.StartsWith("/api") && path.TrimEnd('/') == "/api")
            {
                if (method == "GET")
                {
                    string htmlFilePath = @"E:\Project\ClientServerArch\Documentation\Documentation.html";
                    string htmlContent = File.ReadAllText(htmlFilePath);
                    await Settings.SendResponse(response, htmlContent, "text/html", HttpStatusCode.OK);
                }
            }
            else if (path.StartsWith("/api/MedicalCards"))
            {
                switch (method)
                {
                    case "GET":
                        Settings.Log($"Received {request.HttpMethod} request on {request.Url.AbsolutePath}", ConsoleColor.DarkGray);
                        await MedicalCardsRequests.HandleGetMedicalCard(response);
                        break;
                    case "POST":
                        Settings.Log($"Received {request.HttpMethod} request on {request.Url.AbsolutePath}", ConsoleColor.DarkGray);
                        await MedicalCardsRequests.HandlePostMedicalCard(request, response);
                        break;
                    case "PUT":
                        Settings.Log($"Received {request.HttpMethod} request on {request.Url.AbsolutePath}", ConsoleColor.DarkGray);
                        await MedicalCardsRequests.HandlePutMedicalCard(request, response);
                        break;
                    case "DELETE":
                        Settings.Log($"Received {request.HttpMethod} request on {request.Url.AbsolutePath}", ConsoleColor.DarkGray);
                        await MedicalCardsRequests.HandleDeleteMedicalCard(request, response);
                        break;
                    default:
                        response.StatusCode = 404;
                        break;
                }
            }
            else
            {
                Settings.Log("Resources not found", ConsoleColor.DarkRed, HttpStatusCode.NotFound);
                response.StatusCode = 404;
            }
            response.Close();
        }

        private static async Task StartServer()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8000/api/");
            listener.Start();
            Settings.Log("Server started listening on http://localhost:8000/api/", ConsoleColor.DarkGray);

            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                await RouteRequest(context.Request, context.Response);
            }
        }
        static void Main(string[] args)
        {
            Task.Run(() => StartServer()).GetAwaiter().GetResult();
        }
    }
}
