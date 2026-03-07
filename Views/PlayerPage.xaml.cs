using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyMovie;
using System;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Storage;

namespace MyMovie.Views
{
    public sealed partial class PlayerPage : Page
    {
        public PlayerPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Chuyển đổi chế độ toàn màn hình bằng AppWindow
        /// </summary>
        private void ToggleFullScreen()
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.m_window);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            if (appWindow.Presenter.Kind == Microsoft.UI.Windowing.AppWindowPresenterKind.FullScreen)
            {
                appWindow.SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.Default);
            }
            else
            {
                appWindow.SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.FullScreen);
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string videoPath && !string.IsNullOrEmpty(videoPath))
            {
                await StartPlayback(videoPath);
            }
        }

        private async Task StartPlayback(string path)
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(path);
                MainPlayer.Source = MediaSource.CreateFromStorageFile(file);
                MainPlayer.MediaPlayer.Play();
            }
            catch (Exception)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Lỗi phát video",
                    Content = "Không thể tìm thấy tệp phim hoặc định dạng không hỗ trợ.",
                    CloseButtonText = "Đóng",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Thoát chế độ toàn màn hình nếu đang bật trước khi quay lại
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.m_window);
            var appWindow = AppWindow.GetFromWindowId(Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd));
            if (appWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen)
            {
                appWindow.SetPresenter(AppWindowPresenterKind.Default);
            }

            MainPlayer.MediaPlayer.Pause();
            MainPlayer.Source = null;

            if (Frame.CanGoBack) Frame.GoBack();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (MainPlayer.MediaPlayer != null)
            {
                MainPlayer.MediaPlayer.Pause();
            }
            base.OnNavigatedFrom(e);
        }
    }
}