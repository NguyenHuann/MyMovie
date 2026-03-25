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

            // 2. Cài đặt Icon giao diện theo Theme hiện tại
            InitializeThemeIcon();

            // 3. Lắng nghe thông báo đổi tên từ trang Settings
            WeakReferenceMessenger.Default.Register<UsernameChangedMessage>(this, (r, m) =>
            {
                UpdateGreeting(m.Value);
            });

            // 4. Thiết lập khi trang nạp xong
            this.Loaded += (s, e) => {
                // Mặc định mở trang Home nếu chưa có trang nào trong Frame
                if (ContentFrame.Content == null)
                {
                    NavView.SelectedItem = NavView.MenuItems[0];
                    ContentFrame.Navigate(typeof(HomePage));
                }
            };
        }

        #region Các nút trên Header (Giao diện, Tìm kiếm, Sắp xếp)

        private void InitializeThemeIcon()
        {
            var savedTheme = ApplicationData.Current.LocalSettings.Values["AppTheme"]?.ToString();
            if (savedTheme == "Light")
            {
                // Đang ở nền sáng -> Hiện mặt trăng gợi ý đổi sang tối
                ThemeIcon.Glyph = "\xE708";
            }
            else
            {
                // Đang ở nền tối (hoặc mặc định) -> Hiện mặt trời gợi ý đổi sang sáng
                ThemeIcon.Glyph = "\xE706";
            }
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.m_window?.Content is FrameworkElement rootElement)
            {
                // Lấy theme hiện tại đang hiển thị
                var currentTheme = rootElement.RequestedTheme;

                // Kiểm tra xem đang ở nền Tối hay Sáng
                var savedTheme = ApplicationData.Current.LocalSettings.Values["AppTheme"]?.ToString();
                bool isCurrentlyDark = currentTheme == ElementTheme.Dark || (currentTheme == ElementTheme.Default && savedTheme != "Light");

                if (isCurrentlyDark)
                {
                    // Nếu đang Tối -> Đổi sang Sáng
                    rootElement.RequestedTheme = ElementTheme.Light;
                    ApplicationData.Current.LocalSettings.Values["AppTheme"] = "Light";
                    ThemeIcon.Glyph = "\xE708"; // Đổi icon thành Mặt trăng
                }
                else
                {
                    // Nếu đang Sáng -> Đổi sang Tối
                    rootElement.RequestedTheme = ElementTheme.Dark;
                    ApplicationData.Current.LocalSettings.Values["AppTheme"] = "Dark";
                    ThemeIcon.Glyph = "\xE706"; // Đổi icon thành Mặt trời
                }
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            GreetingText.Visibility = Visibility.Collapsed;
            ThemeButton.Visibility = Visibility.Collapsed; // Ẩn nút Theme
            SearchButton.Visibility = Visibility.Collapsed;
            SortButton.Visibility = Visibility.Collapsed;

            HeaderSearchBox.Visibility = Visibility.Visible;
            CloseSearchButton.Visibility = Visibility.Visible;

            HeaderSearchBox.Focus(FocusState.Programmatic);
        }

        private void CloseSearchButton_Click(object sender, RoutedEventArgs e)
        {
            HeaderSearchBox.Visibility = Visibility.Collapsed;
            HeaderSearchBox.Text = string.Empty;
            CloseSearchButton.Visibility = Visibility.Collapsed;

            GreetingText.Visibility = Visibility.Visible;
            ThemeButton.Visibility = Visibility.Visible; // Hiện lại nút Theme
            SearchButton.Visibility = Visibility.Visible;
            SortButton.Visibility = Visibility.Visible;

            // PHÁT SÓNG TÍN HIỆU: Reset lại danh sách phim khi đóng thanh tìm kiếm
            WeakReferenceMessenger.Default.Send(new SearchMessage(""));
        }

        private void HeaderSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                // PHÁT SÓNG TÍN HIỆU: Gửi từ khóa người dùng gõ xuống HomePage
                WeakReferenceMessenger.Default.Send(new SearchMessage(sender.Text.ToLower()));
            }
        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            // Tạo Menu thả xuống cho nút Sắp xếp
            var flyout = new MenuFlyout();

            var sortByNameAsc = new MenuFlyoutItem { Text = "Tên phim (A đến Z)", Icon = new FontIcon { Glyph = "\xE74B" } };
            sortByNameAsc.Click += (s, args) => WeakReferenceMessenger.Default.Send(new SortMessage("NameAsc"));

            var sortByNameDesc = new MenuFlyoutItem { Text = "Tên phim (Z đến A)", Icon = new FontIcon { Glyph = "\xE74A" } };
            sortByNameDesc.Click += (s, args) => WeakReferenceMessenger.Default.Send(new SortMessage("NameDesc"));

            flyout.Items.Add(sortByNameAsc);
            flyout.Items.Add(sortByNameDesc);

            flyout.ShowAt((FrameworkElement)sender);
        }

        #endregion

        #region Logic Lời chào (Greeting)

        private void UpdateGreeting(string? userName = null)
        {
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
            if (hour >= 5 && hour < 12) return "Morning";
            if (hour >= 12 && hour < 18) return "Afternoon";
            return "Evening";
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

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            NavView.IsBackEnabled = ContentFrame.CanGoBack;

            // Ẩn các nút Tìm kiếm/Sắp xếp khi vào các trang phụ để tránh lỗi
            if (e.SourcePageType == typeof(AddMoviePage) ||
                e.SourcePageType == typeof(SettingsPage) ||
                e.SourcePageType == typeof(MovieDetailsPage))
            {
                SearchButton.Visibility = Visibility.Collapsed;
                SortButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                SearchButton.Visibility = Visibility.Visible;
                SortButton.Visibility = Visibility.Visible;
            }

            if (e.SourcePageType.Name.Contains("PlayerPage"))
            {
                HeaderPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                HeaderPanel.Visibility = Visibility.Visible;
                if (HeaderSearchBox.Visibility == Visibility.Visible)
                {
                    CloseSearchButton_Click(null!, null!);
                }
                UpdateGreeting();
            }
        }

        #endregion
    }
}