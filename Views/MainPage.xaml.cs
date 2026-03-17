using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;
using Windows.Storage;
using CommunityToolkit.Mvvm.Messaging;
using MyMovie.Models;

namespace MyMovie.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            // 1. Khởi tạo câu chào ban đầu
            UpdateGreeting();

            // 2. Lắng nghe thông báo đổi tên từ trang Settings
            WeakReferenceMessenger.Default.Register<UsernameChangedMessage>(this, (r, m) =>
            {
                UpdateGreeting(m.Value);
            });

            // 3. Thiết lập khi trang nạp xong
            this.Loaded += (s, e) => {
                // Mặc định mở trang Home nếu chưa có trang nào trong Frame
                if (ContentFrame.Content == null)
                {
                    NavView.SelectedItem = NavView.MenuItems[0];
                    ContentFrame.Navigate(typeof(HomePage));
                }
            };
        }

        #region Logic Lời chào (Greeting)

        private void UpdateGreeting(string? userName = null)
        {
            // Nếu không truyền tên, lấy từ LocalSettings
            if (string.IsNullOrEmpty(userName))
            {
                userName = ApplicationData.Current.LocalSettings.Values["UserName"]?.ToString() ?? "user";
            }

            string timeGreeting = GetTimeBasedGreeting();
            GreetingText.Text = $"{timeGreeting}, {userName}!";
        }

        private string GetTimeBasedGreeting()
        {
            int hour = DateTime.Now.Hour;
            if (hour >= 5 && hour < 12) return "morning";
            if (hour >= 12 && hour < 18) return "afternoon";
            return "evening";
        }

        #endregion

        #region Điều hướng (Navigation)

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else if (args.SelectedItemContainer != null)
            {
                string tag = args.SelectedItemContainer.Tag.ToString()!;
                Type pageType = tag switch
                {
                    "home" => typeof(HomePage),
                    "favorites" => typeof(FavoritesPage),
                    "history" => typeof(HistoryPage),
                    "settings" => typeof(SettingsPage),
                    _ => typeof(HomePage)
                };

                if (ContentFrame.CurrentSourcePageType != pageType)
                {
                    ContentFrame.Navigate(pageType);
                }
            }
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }

        // Sự kiện đồng bộ hóa sau khi điều hướng thành công
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // Bật/Tắt nút Back dựa trên lịch sử duyệt trang
            NavView.IsBackEnabled = ContentFrame.CanGoBack;

            // Tự động ẩn Header (Lời chào/Search) khi vào trang xem phim để rộng chỗ
            if (e.SourcePageType.Name.Contains("PlayerPage"))
            {
                HeaderPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                HeaderPanel.Visibility = Visibility.Visible;
                UpdateGreeting(); // Cập nhật lại lời chào mỗi khi quay lại trang chính
            }
        }

        #endregion
    }
}