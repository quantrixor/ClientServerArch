using Microsoft.Win32;
using ModelDataBase.Model;
using Newtonsoft.Json;
using QRCoder;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DesktopClient.ViewModel
{
    internal class ViewModelMedicalCards
    {
        public string FilePath { get; private set; }
        public BitmapImage image { get; set; }

        // Метод для отправки данных на сервер и получения ответа
        public async Task<HttpResponseMessage> SendDataToServerAsync(MedicalCardCodes medicalCard)
        {
            using (var client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(medicalCard);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:8000/api/MedicalCards", content);
                return response;
            }
        }
        public string code { get; set; }
        // Метод для попытки добавить уникальную медицинскую карту
        public async Task<bool> TryAddUniqueMedicalCardAsync()
        {
            bool isUnique = false;
            HttpResponseMessage response;

            do
            {
                code = GenerateCode();
                image = GenerateQRCode(code);
                string fileName = $"{code}.png";

                var medicalCard = new MedicalCardCodes
                {
                    Code = code,
                    // jksdfjsdfjsdljfl
                    PathQRCode = fileName
                };

                response = await SendDataToServerAsync(medicalCard);

                if (response.IsSuccessStatusCode)
                {
                    isUnique = true;
                }
                else if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    // Если код уже существует, цикл продолжится для генерации нового кода
                    File.Delete(FilePath); // Удаляем созданный файл, так как код не уникален
                }
                else
                {
                    // Обработка других возможных ошибок HTTP
                    throw new HttpRequestException($"Error: {response.ReasonPhrase}");
                }
            } while (!isUnique);

            return isUnique;
        }

        public string GenerateCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public BitmapImage GenerateQRCode(string code)
        {
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(code, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new QRCode(qrCodeData))
            using (var qrCodeImage = qrCode.GetGraphic(20))
            using (var ms = new MemoryStream())
            {
                qrCodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        public void SaveQRCodeToFile(BitmapImage qrImage, string fileName)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG Files (*.png)|*.png",
                DefaultExt = "png",
                AddExtension = true,
                FileName = fileName
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                FilePath = saveFileDialog.FileName;
                using (var fileStream = new FileStream(FilePath, FileMode.Create))
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(qrImage));
                    encoder.Save(fileStream);
                }
            }
        }
    }
}
