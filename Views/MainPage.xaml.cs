using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Storage;
using Windows.UI.ApplicationSettings;
using CommunityToolkit.Mvvm.Messaging;
using MyMovie.Models;

namespace MyMovie.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            string currentUserName = Windows.Storage.ApplicationData.Current.LocalSettings.Values["UserName"] as string ?? "user";
            UpdateGreeting(currentUserName);

            // Cập nhật tức thì khi người dùng đổi tên ở trang Settings
            WeakReferenceMessenger.Default.Register<UsernameChangedMessage>(this, (r, m) =>
            {
                // Sử dụng hàm UpdateGreeting để đảm bảo tính đúng đắn về thời gian
                UpdateGreeting(m.Value);
            });

            // Cập nhật câu chào khi trang vừa tải xong
            this.Loaded += (s, e) => {
                UpdateGreeting();
                // Mặc định điều hướng vào trang Home khi mở app
                NavView.SelectedItem = NavView.MenuItems[0];
            };
        }

        /// <summary>
        /// Cập nhật câu chào cá nhân hóa dựa trên thời gian và tên người dùng
        /// </summary>

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            // Kiểm tra xem Frame có trang nào để quay lại không
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }
        private void UpdateGreeting()
        {
            // Lấy tên người dùng từ LocalSettings (đã lưu từ trang Settings)
            var settings = ApplicationData.Current.LocalSettings;
            string userName = settings.Values["UserName"]?.ToString() ?? "user";

            var hour = DateTime.Now.Hour;
            string greetingPrefix = hour switch
            {
                < 12 => "morning",
                < 18 => "afternoon",
                _ => "evening"
            };

            // Kết quả hiển thị: "morning, user!"
            GreetingText.Text = $"{greetingPrefix}, {userName}!";
        }

        /// <summary>
        /// Xử lý điều hướng khi người dùng nhấn vào các mục trên Sidebar
        /// </summary>
        private void UpdateGreeting(string userName)
        {
            string timeGreeting = GetTimeBasedGreeting();
            GreetingText.Text = $"{timeGreeting}, {userName}!";
        }
        private string GetTimeBasedGreeting()
        {
            int hour = DateTime.Now.Hour;

            if (hour >= 5 && hour < 12)
            {
                return "morning"; // Từ 5h sáng đến trước 12h trưa
            }
            else if (hour >= 12 && hour < 18)
            {
                return "afternoon"; // Từ 12h trưa đến trước 6h chiều
            }
            else
            {
                return "evening"; // Từ 6h chiều đến 5h sáng hôm sau
            }
        }
        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                string tag = args.SelectedItemContainer.Tag.ToString();

                switch (tag)
                {
                    case "home":
                        ContentFrame.Navigate(typeof(HomePage));
                        break;
                    case "favorites":
                        ContentFrame.Navigate(typeof(FavoritesPage));
                        break;
                    case "history":
                        ContentFrame.Navigate(typeof(HistoryPage));
                        break; 
                    case "settings":
                        ContentFrame.Navigate(typeof(SettingsPage));
                        break;
                }
            }
        }
    }
}