using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using MyMovie.Models;
using MyMovie.Data;

namespace MyMovie.Views
{
    public sealed partial class AddMoviePage : Page
    {
        public AddMoviePage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Mở trình chọn tệp video (.mp4, .mkv, .avi)
        /// </summary>
        private async void PickVideo_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();

            // Lấy Window Handle để Picker hiển thị đúng lớp trên WinUI 3
            var hWnd = WindowNative.GetWindowHandle(App.m_window);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            InitializeWithWindow.Initialize(picker, hWnd);

            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".mkv");
            picker.FileTypeFilter.Add(".avi");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Lưu đường dẫn tuyệt đối để trình phát video có thể truy cập sau này
                VideoPathDisplay.Text = file.Path;

                // Tự động điền tên phim nếu người dùng chưa nhập
                if (string.IsNullOrWhiteSpace(TitleInput.Text))
                {
                    TitleInput.Text = file.DisplayName;
                }
            }
        }

        /// <summary>
        /// Mở trình chọn ảnh bìa cho phim
        /// </summary>
        private async void PickPoster_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            var hWnd = WindowNative.GetWindowHandle(App.m_window);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            InitializeWithWindow.Initialize(picker, hWnd);

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpeg");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                PosterPathDisplay.Text = file.Path;

                // Hiển thị ảnh xem trước trực tiếp từ tệp
                BitmapImage bitmap = new BitmapImage();
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    await bitmap.SetSourceAsync(stream);
                }
                PosterPreview.Source = bitmap;
                PosterPreview.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Lưu dữ liệu vào SQLite và quay về trang chủ
        /// </summary>
        private async void SaveMovie_Click(object sender, RoutedEventArgs e)
        {
            // 1. Kiểm tra các trường bắt buộc
            if (string.IsNullOrWhiteSpace(TitleInput.Text) || string.IsNullOrWhiteSpace(VideoPathDisplay.Text))
            {
                ShowErrorDialog("Vui lòng nhập tên phim và chọn tệp video.");
                return;
            }

            try
            {
                // 2. Tạo đối tượng phim mới với dữ liệu "sạch"
                var newMovie = new Movie
                {
                    Title = TitleInput.Text.Trim(),
                    Director = DirectorInput.Text?.Trim() ?? "Chưa rõ",

                    // CẬP NHẬT: Lấy thể loại từ ComboBoxItem
                    Genre = (GenreInput.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Chưa phân loại",

                    Description = DescInput.Text?.Trim() ?? "",
                    VideoPath = VideoPathDisplay.Text,
                    ThumbnailPath = PosterPathDisplay.Text ?? "", // Poster có thể trống
                    DateAdded = DateTime.Now
                };

                // 3. Lưu vào SQLite
                using (var db = new AppDbContext())
                {
                    db.Movies.Add(newMovie);
                    await db.SaveChangesAsync();
                }

                // 4. Quay lại HomePage sau khi lưu thành công
                if (Frame.CanGoBack) Frame.GoBack();
            }
            catch (Exception ex)
            {
                ShowErrorDialog($"Lỗi khi lưu phim: {ex.Message}");
            }
        }

        private async void ShowErrorDialog(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Thông báo",
                Content = message,
                CloseButtonText = "Đóng",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack) Frame.GoBack();
        }
    }
}