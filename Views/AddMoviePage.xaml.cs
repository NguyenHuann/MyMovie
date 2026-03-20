using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;
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
        // Lưu trữ đối tượng phim nếu đang ở chế độ Chỉnh sửa
        private Movie _editingMovie = null;

        public AddMoviePage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Nhận dữ liệu phim và thiết lập giao diện (Thêm hoặc Sửa)
        /// </summary>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Movie movie)
            {
                _editingMovie = movie;

                // 1. Đổ dữ liệu vào các ô nhập liệu
                TitleInput.Text = _editingMovie.Title ?? "";
                DirectorInput.Text = _editingMovie.Director ?? "";
                DescInput.Text = _editingMovie.Description ?? "";
                VideoPathDisplay.Text = _editingMovie.VideoPath ?? "";
                PosterPathDisplay.Text = _editingMovie.ThumbnailPath ?? "";

                // 2. Chọn Thể loại an toàn trong ComboBox
                if (GenreInput != null && !string.IsNullOrEmpty(_editingMovie.Genre))
                {
                    GenreInput.SelectedItem = GenreInput.Items
                        .OfType<ComboBoxItem>()
                        .FirstOrDefault(x => x.Content?.ToString() == _editingMovie.Genre);
                }

                // 3. Nạp ảnh an toàn (Fix crash Win32)
                if (!string.IsNullOrEmpty(_editingMovie.ThumbnailPath))
                {
                    await LoadImageAsync(_editingMovie.ThumbnailPath);
                }

                // 4. Cập nhật Text giao diện cho chế độ Sửa
                PageTitle.Text = "Chỉnh sửa phim";
                SaveBtn.Content = "Cập nhật";
            }
        }

        /// <summary>
        /// Hàm nạp ảnh an toàn bằng Stream để tránh lỗi Crash "Win32 Unhandled Exception"
        /// </summary>
        private async Task LoadImageAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                // Kiểm tra file tồn tại trước khi nạp
                StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(stream);
                    PosterPreview.Source = bitmap;

                    // HIỆN ảnh và nút Đổi ảnh, ẨN icon hướng dẫn ban đầu
                    UploadPlaceholder.Visibility = Visibility.Collapsed;
                    ImageSelectedContent.Visibility = Visibility.Visible;
                }
            }
            catch (Exception)
            {
                // Nếu lỗi (file bị xóa/không truy cập được), hiện lại trạng thái chọn ảnh trống
                UploadPlaceholder.Visibility = Visibility.Visible;
                ImageSelectedContent.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Xử lý chọn Ảnh bìa (Poster)
        /// </summary>
        private async void PickPoster_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            var hWnd = WindowNative.GetWindowHandle(App.m_window);
            InitializeWithWindow.Initialize(picker, hWnd);

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpeg");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                PosterPathDisplay.Text = file.Path;
                await LoadImageAsync(file.Path); // Nạp và đổi trạng thái giao diện
            }
        }

        /// <summary>
        /// Xử lý chọn Video
        /// </summary>
        private async void PickVideo_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            var hWnd = WindowNative.GetWindowHandle(App.m_window);
            InitializeWithWindow.Initialize(picker, hWnd);

            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".mkv");
            picker.FileTypeFilter.Add(".avi");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                VideoPathDisplay.Text = file.Path;
                if (string.IsNullOrWhiteSpace(TitleInput.Text)) TitleInput.Text = file.DisplayName;
            }
        }

        /// <summary>
        /// Xử lý Lưu hoặc Cập nhật vào SQLite
        /// </summary>
        private async void SaveMovie_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleInput.Text) || string.IsNullOrWhiteSpace(VideoPathDisplay.Text))
            {
                ShowErrorDialog("Vui lòng nhập tên phim và chọn tệp video.");
                return;
            }

            try
            {
                using var db = new AddDbContext(); // Đảm bảo đúng tên Class DbContext của bạn

                if (_editingMovie != null)
                {
                    // CHẾ ĐỘ CẬP NHẬT
                    var movieInDb = db.Movies.FirstOrDefault(m => m.Id == _editingMovie.Id);
                    if (movieInDb != null)
                    {
                        movieInDb.Title = TitleInput.Text.Trim();
                        movieInDb.Director = DirectorInput.Text?.Trim();
                        movieInDb.Genre = (GenreInput.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Chưa rõ";
                        movieInDb.Description = DescInput.Text?.Trim();
                        movieInDb.VideoPath = VideoPathDisplay.Text;
                        movieInDb.ThumbnailPath = PosterPathDisplay.Text;

                        db.Movies.Update(movieInDb);

                        // Đồng bộ lại object tham chiếu để các trang khác nhận thay đổi ngay
                        _editingMovie.Title = movieInDb.Title;
                        _editingMovie.Director = movieInDb.Director;
                        _editingMovie.Genre = movieInDb.Genre;
                        _editingMovie.Description = movieInDb.Description;
                        _editingMovie.VideoPath = movieInDb.VideoPath;
                        _editingMovie.ThumbnailPath = movieInDb.ThumbnailPath;
                    }
                }
                else
                {
                    // CHẾ ĐỘ THÊM MỚI
                    var newMovie = new Movie
                    {
                        Title = TitleInput.Text.Trim(),
                        Director = DirectorInput.Text?.Trim(),
                        Genre = (GenreInput.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Chưa rõ",
                        Description = DescInput.Text?.Trim(),
                        VideoPath = VideoPathDisplay.Text,
                        ThumbnailPath = PosterPathDisplay.Text,
                        DateAdded = DateTime.Now
                    };
                    db.Movies.Add(newMovie);
                }

                await db.SaveChangesAsync();
                Frame.GoBack();
            }
            catch (Exception ex)
            {
                ShowErrorDialog($"Lỗi hệ thống: {ex.Message}");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e) => Frame.GoBack();
        private void Cancel_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

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
    }
}