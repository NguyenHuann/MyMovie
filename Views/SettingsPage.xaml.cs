using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using CommunityToolkit.Mvvm.Messaging;
using MyMovie.Models;

namespace MyMovie.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            // Chỉ tải tên người dùng, đã dọn dẹp phần kiểm tra Giao diện cũ
            var currentName = ApplicationData.Current.LocalSettings.Values["UserName"]?.ToString();
            if (!string.IsNullOrEmpty(currentName))
            {
                UsernameTextBox.Text = currentName;
            }
        }

        private void SaveNameButton_Click(object sender, RoutedEventArgs e)
        {
            var newName = UsernameTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(newName))
            {
                ApplicationData.Current.LocalSettings.Values["UserName"] = newName;
                WeakReferenceMessenger.Default.Send(new UsernameChangedMessage(newName));
            }
        }
    }
}