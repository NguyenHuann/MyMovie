using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace MyMovie.Views
{
    public sealed partial class PlayerPage : Page
    {
        private AppWindow _appWindow;

        public PlayerPage()
        {
            this.InitializeComponent();

            // Khởi tạo AppWindow an toàn
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.m_window);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            _appWindow = AppWindow.GetFromWindowId(windowId);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string videoPath)
            {
                MainPlayer.Source = Windows.Media.Core.MediaSource.CreateFromUri(new Uri(videoPath));

                // PHÓNG TOÀN MÀN HÌNH THỰC THỤ
                // Điều này sẽ che cả thanh Taskbar và Sidebar của bạn
                _appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // BẮT BUỘC: Trả về chế độ cửa sổ trước khi thoát trang
            _appWindow.SetPresenter(AppWindowPresenterKind.Default);

            if (Frame.CanGoBack) Frame.GoBack();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Đảm bảo luôn thoát full màn hình nếu người dùng dùng phím tắt điều hướng
            _appWindow.SetPresenter(AppWindowPresenterKind.Default);
            MainPlayer.MediaPlayer.Pause();
            base.OnNavigatedFrom(e);
        }
    }
}