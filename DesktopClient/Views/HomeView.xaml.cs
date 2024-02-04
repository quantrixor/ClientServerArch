using DesktopClient.ViewModel;
using ModelDataBase.Model;
using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace DesktopClient.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : Page
    {
        public HomeView()
        {
            InitializeComponent();
            buttonSaveData.IsEnabled = false;
        }
        ViewModelMedicalCards viewModel { get; set; }

        private async void buttonGenerateQRCode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                viewModel = new ViewModelMedicalCards();
                bool isUnique = await viewModel.TryAddUniqueMedicalCardAsync();
                var image = viewModel.image;
                var code = viewModel.code;
                if (isUnique)
                {
                    MessageBox.Show("Уникальный код сгенерирован и проверен на сервере.", "Сохранено в базу данных.", MessageBoxButton.OK, MessageBoxImage.Information);
                    viewModel.SaveQRCodeToFile(image, code);
                    pictureQRCode.Source = viewModel.image;
                    buttonSaveData.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("Не удалось сгенерировать уникальный код.", "Уведомление о дублировании.", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
        }

        private async void buttonSaveData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (viewModel == null)
                    return;

                HttpResponseMessage response = await viewModel.SendDataToServerAsync(new MedicalCardCodes
                {
                    Code = viewModel.code,
                    PathQRCode = viewModel.FilePath
                });

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Данные успешно сохранены в базу данных.");
                }
                else
                {
                    MessageBox.Show("Не удалось сохранить данные. Произошла попытка дублирования уникального кода медицинской карты." + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
            finally
            {
                buttonSaveData.IsEnabled = false;
            }
        }

    }
}
