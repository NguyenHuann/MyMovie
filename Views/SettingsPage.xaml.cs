using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Storage;
using CommunityToolkit.Mvvm.Messaging;
using MyMovie.Models;

namespace MyMovie.Views
{
    public sealed partial class SettingsPage : Page
    {
        // Khai báo bộ nhớ cài đặt cục bộ của ứng dụng
        private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;

        public SettingsPage()
        {
            this.InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            // Tải tên người dùng đã lưu (nếu có)
            // Nếu kết quả là null, nó sẽ lấy giá trị "user"
            string savedName = _localSettings.Values["UserName"] as string ?? "user";
            UserNameInput.Text = savedName;
            if (!string.IsNullOrEmpty(savedName))
            {
                UserNameInput.Text = savedName;
            }
        }

        private async void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            string newName = UserNameInput.Text.Trim();
            _localSettings.Values["UserName"] = newName;

            // Gửi thông báo đến toàn bộ ứng dụng
            WeakReferenceMessenger.Default.Send(new UsernameChangedMessage(newName));

            // Lưu vào bộ nhớ máy
            _localSettings.Values["UserName"] = newName;

            // Hiển thị thông báo thành công
            ContentDialog successDialog = new ContentDialog
            {
                Title = "Thành công",
                Content = "Thông tin cài đặt đã được cập nhật.",
                CloseButtonText = "Đóng",
                XamlRoot = this.XamlRoot
            };
            await successDialog.ShowAsync();
        }
    }
}