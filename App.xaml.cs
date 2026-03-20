using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Storage;

namespace MyMovie
{
    public partial class App : Application
    {
        // Thêm dấu ? để sửa cảnh báo vàng CS8618 (cho phép biến rỗng lúc khởi tạo)
        public static Window? m_window;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // Trả lại đúng tên AddDbContext nguyên bản để sửa lỗi đỏ CS0234
            using (var db = new MyMovie.Data.AddDbContext())
            {
                db.Database.EnsureCreated();
            }

            m_window = new Window();
            m_window.Content = new Views.MainPage();
            m_window.Title = "MyMovie";

            // ĐỌC VÀ ÁP DỤNG THEME TRƯỚC KHI HIỂN THỊ CỬA SỔ
            if (m_window.Content is FrameworkElement rootElement)
            {
                // Thêm dấu ? cho biến savedTheme để sửa cảnh báo vàng CS8600
                string? savedTheme = ApplicationData.Current.LocalSettings.Values["AppTheme"] as string;

                if (savedTheme == "Dark")
                {
                    rootElement.RequestedTheme = ElementTheme.Dark;
                }
                else if (savedTheme == "Light")
                {
                    rootElement.RequestedTheme = ElementTheme.Light;
                }
            }

            m_window.Activate();
        }
    }
}