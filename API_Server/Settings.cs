using System;
using System.Data.Entity.Validation;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace API_Server
{
    internal static class Settings
    {
        internal static string FormatValidationErrorMessage(DbEntityValidationException dbEx)
        {
            var errorMessageBuilder = new StringBuilder();
            foreach (var validationErrors in dbEx.EntityValidationErrors)
            {
                foreach (var validationError in validationErrors.ValidationErrors)
                {
                    errorMessageBuilder.AppendLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                }
            }
            return errorMessageBuilder.ToString();
        }

        internal static async Task SendResponse(HttpListenerResponse response, string content, string contentType = "text/html", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            byte[] data = Encoding.UTF8.GetBytes(content);

            response.ContentType = contentType;
            response.ContentLength64 = data.Length;
            response.StatusCode = (int)statusCode;

            await response.OutputStream.WriteAsync(data, 0, data.Length);
            response.Close();
        }

        internal static void Log(string message, ConsoleColor consoleColor = ConsoleColor.Green, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine($"{DateTime.Now} - {message} status code: {statusCode}");
        }

    }
}
