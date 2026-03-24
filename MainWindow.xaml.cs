using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.IO;

namespace MyMovie
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            // 1. CẤU HÌNH ICON (Fix lỗi không hiện ở Title Bar)
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            if (appWindow != null)
            {
                // Dùng đường dẫn tuyệt đối cho chắc chắn
                string iconPath = Path.Combine(AppContext.BaseDirectory, "Assets/movie.ico");
                if (File.Exists(iconPath))
                {
                    appWindow.SetIcon(iconPath);
                }
            }

            // 2. ĐIỀU HƯỚNG TỚI MAINPAGE (Trang "Morning, huan!")
            // Điều này giúp MainWindow không bị trống khi vừa mở App
            RootFrame.Navigate(typeof(Views.MainPage));
        }
    }
}