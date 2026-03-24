using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Storage;

namespace MyMovie
{
    public partial class App : Application
    {
        public static Window? m_window;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // 1. Khởi tạo Database
            using (var db = new MyMovie.Data.AddDbContext())
            {
                db.Database.EnsureCreated();
            }

            // 2. PHẢI dùng MainWindow (nơi bạn viết code đặt Icon)
            m_window = new MainWindow();

            // 3. Thiết lập tiêu đề (Sẽ bị ghi đè nếu MainWindow có code riêng)
            m_window.Title = "MyMovie";

            // 4. Áp dụng Theme (Thực hiện trên Content của MainWindow)
            if (m_window.Content is FrameworkElement rootElement)
            {
                string? savedTheme = ApplicationData.Current.LocalSettings.Values["AppTheme"] as string;
                rootElement.RequestedTheme = savedTheme switch
                {
                    "Dark" => ElementTheme.Dark,
                    "Light" => ElementTheme.Light,

                };
            }

            m_window.Activate();
        }
    }
}