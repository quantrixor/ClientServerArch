using ModelDataBase.Model;
using Newtonsoft.Json;
using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace API_Server
{
    internal static class MedicalCardsRequests
    {
        /// <summary>
        /// POST Request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        internal static async Task HandlePostMedicalCard(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                string requestBody;

                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                var medicalCard = JsonConvert.DeserializeObject<MedicalCardCodes>(requestBody);

                if (medicalCard != null)
                {
                    using (var db = new dbModel())
                    {
                        //Проверяем наличие медицинской карты с таким же кодом
                        var existingCard = db.MedicalCardCodes.FirstOrDefault(mc => mc.Code == medicalCard.Code);
                        if (existingCard == null)
                        {
                            db.MedicalCardCodes.Add(medicalCard);
                            await db.SaveChangesAsync();
                            Settings.Log("New medical card save successfully! Unique medical card code: " + medicalCard.Code, ConsoleColor.Green, HttpStatusCode.Created); // Логирование успешного добавления
                            await Settings.SendResponse(response, "New medical card added successfully.", "application/json", HttpStatusCode.Created);
                        }
                        else
                        {
                            Settings.Log("Attempt to add duplicate code: " + medicalCard.Code, ConsoleColor.DarkRed, HttpStatusCode.Conflict); // Логирование попытки добавления дубликата
                            await Settings.SendResponse(response, "The code already exists.", "application/json", HttpStatusCode.Conflict);
                        }
                    }
                }
                else
                {
                    Settings.Log("Invalid medical card data", ConsoleColor.DarkRed, HttpStatusCode.BadRequest);
                    await Settings.SendResponse(response, "Invalid medical card data.", "application/json", HttpStatusCode.BadRequest);
                }
            }
            catch (JsonException jsonEx)
            {
                Settings.Log($"{response}, Invalid JSON format: ${jsonEx.Message}", ConsoleColor.DarkRed, HttpStatusCode.BadRequest);
                await Settings.SendResponse(response, $"Invalid JSON format: {jsonEx.Message}", "application/json", HttpStatusCode.BadRequest);
            }
            catch (DbEntityValidationException dbEx)
            {
                var errorMessage = Settings.FormatValidationErrorMessage(dbEx);
                Settings.Log(errorMessage, ConsoleColor.DarkRed, HttpStatusCode.BadRequest);
                await Settings.SendResponse(response, $"Validation error: {errorMessage}", "application/json", HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// PUT Request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        internal static async Task HandlePutMedicalCard(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                string requestBody;

                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                var medicalCard = JsonConvert.DeserializeObject<MedicalCardCodes>(requestBody);

                if (medicalCard != null)
                {
                    using (var db = new dbModel())
                    {
                        var existingCard = await db.MedicalCardCodes.FindAsync(medicalCard.Id);

                        if (existingCard != null)
                        {
                            // Проверяем, есть ли дубликат нового кода
                            var dublicateCard = await db.MedicalCardCodes.Where(mc => mc.Code == medicalCard.Code)
                                .FirstOrDefaultAsync();

                            if(dublicateCard != null)
                            {
                                // Найден дубликат
                                Settings.Log("Duplicate medical card code: " + medicalCard.Code, ConsoleColor.DarkRed, HttpStatusCode.Conflict);
                                await Settings.SendResponse(response, "A medical card with the same code already exists.", "application/json", HttpStatusCode.Conflict);
                                return;

                            }
                            
                            // Обновляем существующую запись данными из запроса
                            db.Entry(existingCard).CurrentValues.SetValues(medicalCard);
                            await db.SaveChangesAsync();
                            Settings.Log("Medical card updated successfully! Unique medical card code: " + medicalCard.Code, ConsoleColor.Green, HttpStatusCode.OK); // Логирование успешного обновления
                            await Settings.SendResponse(response, "Medical card updated successfully.", "application/json", HttpStatusCode.OK);
                        }
                        else
                        {
                            // Сообщаем об отсутствии карты с таким ID
                            Settings.Log("Medical card with code " + medicalCard.Id + " not found.", ConsoleColor.DarkRed, HttpStatusCode.NotFound);
                            await Settings.SendResponse(response, "Medical card with the specified ID does not exist.", "application/json", HttpStatusCode.NotFound);
                        }
                    }
                }
                else
                {
                    Settings.Log("Invalid medical card data", ConsoleColor.DarkRed, HttpStatusCode.BadRequest);
                    await Settings.SendResponse(response, "Invalid medical card data.", "application/json", HttpStatusCode.BadRequest);
                }
            }
            catch (JsonException jsonEx)
            {
                Settings.Log($"Invalid JSON format: {jsonEx.Message}", ConsoleColor.DarkRed, HttpStatusCode.BadRequest);
                await Settings.SendResponse(response, $"Invalid JSON format: {jsonEx.Message}", "application/json", HttpStatusCode.BadRequest);
            }
            catch (DbEntityValidationException dbEx)
            {
                var errorMessage = Settings.FormatValidationErrorMessage(dbEx);
                Settings.Log(errorMessage, ConsoleColor.DarkRed, HttpStatusCode.BadRequest);
                await Settings.SendResponse(response, $"Validation error: {errorMessage}", "application/json", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// GET Request
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        internal static async Task HandleGetMedicalCard(HttpListenerResponse response)
        {
            try
            {
                using (var db = new dbModel())
                {
                    var medicalCards = db.MedicalCardCodes.ToList();
                    string jsonResponse = JsonConvert.SerializeObject(medicalCards);

                    await Settings.SendResponse(response, jsonResponse, "application/json", HttpStatusCode.OK);
                }
            }
            catch (JsonException jsonEx)
            {
                Settings.Log($"{response}, Invalid JSON format: ${jsonEx.Message}", ConsoleColor.DarkRed, HttpStatusCode.BadRequest);
                await Settings.SendResponse(response, $"Invalid JSON format: {jsonEx.Message}", "application/json", HttpStatusCode.BadRequest);
            }
            catch (DbEntityValidationException dbEx)
            {
                var errorMessage = Settings.FormatValidationErrorMessage(dbEx);
                Settings.Log(errorMessage, ConsoleColor.DarkRed, HttpStatusCode.BadRequest);
                await Settings.SendResponse(response, $"Validation error: {errorMessage}", "application/json", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// DELETE Request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        internal static async Task HandleDeleteMedicalCard(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                // Извлечение ID медицинской карты из URL запроса или тела запроса
                // Предположим, что ID передается как часть URL, например: /api/MedicalCards/123
                // Ниже пример того, как вы могли бы извлечь ID, если он передается в URL.
                var urlSegments = request.RawUrl.Split('/');
                var medicalCardId = Convert.ToInt32(urlSegments.Last()); // Простая демонстрация, требует проверки и обработки ошибок

                using (var db = new dbModel())
                {
                    var medicalCard = await db.MedicalCardCodes.FindAsync(medicalCardId);
                    if (medicalCard != null)
                    {
                        db.MedicalCardCodes.Remove(medicalCard);
                        await db.SaveChangesAsync();
                        Settings.Log("Medical card deleted successfully!", ConsoleColor.Green, HttpStatusCode.OK);
                        await Settings.SendResponse(response, "Medical card deleted successfully!", "application/json", HttpStatusCode.OK);
                    }
                    else
                    {
                        Settings.Log("Medical card not found.", ConsoleColor.DarkRed, HttpStatusCode.NotFound);
                        await Settings.SendResponse(response, "Medical card not found.", "application/json", HttpStatusCode.NotFound);
                    }
                }
            }
            catch (FormatException formatEx)
            {
                Settings.Log($"Invalid ID format: {formatEx.Message}", ConsoleColor.DarkRed, HttpStatusCode.BadRequest);
                await Settings.SendResponse(response, "Invalid ID format.", "application/json", HttpStatusCode.BadRequest);
            }
            catch (JsonException jsonEx)
            {
                Settings.Log($"Invalid JSON format: {jsonEx.Message}", ConsoleColor.DarkRed, HttpStatusCode.BadRequest);
                await Settings.SendResponse(response, "Invalid JSON format.", "application/json", HttpStatusCode.BadRequest);
            }
            catch (DbEntityValidationException dbEx)
            {
                var errorMessage = Settings.FormatValidationErrorMessage(dbEx);
                Settings.Log(errorMessage, ConsoleColor.DarkRed, HttpStatusCode.BadRequest);
                await Settings.SendResponse(response, "Validation error: " + errorMessage, "application/json", HttpStatusCode.BadRequest);
            }
        }

    }
}
