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
        // Biến lưu trữ phim đang được chỉnh sửa (nếu điều hướng từ trang Detail)
        private Movie _editingMovie = null;

        public AddMoviePage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Nhận dữ liệu phim khi điều hướng từ trang chi tiết (Chế độ Sửa)
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Nếu tham số truyền sang là một đối tượng Movie (Chế độ Sửa)
            if (e.Parameter is Movie movie)
            {
                _editingMovie = movie;

                // 1. Đổ dữ liệu hiện tại vào các ô nhập liệu
                TitleInput.Text = _editingMovie.Title;
                DirectorInput.Text = _editingMovie.Director; // Thuộc tính đạo diễn mới
                DescInput.Text = _editingMovie.Description;
                VideoPathDisplay.Text = _editingMovie.VideoPath;
                PosterPathDisplay.Text = _editingMovie.ThumbnailPath;

                // 2. Chọn lại đúng Thể loại trong ComboBox
                if (GenreInput != null && !string.IsNullOrEmpty(_editingMovie.Genre))
                {
                    foreach (var item in GenreInput.Items)
                    {
                        if (item is ComboBoxItem cbItem && cbItem.Content.ToString() == _editingMovie.Genre)
                        {
                            GenreInput.SelectedItem = cbItem;
                            break;
                        }
                    }
                }

                // 3. Hiển thị ảnh xem trước nếu đã có đường dẫn ảnh
                if (!string.IsNullOrEmpty(_editingMovie.ThumbnailPath))
                {
                    try
                    {
                        PosterPreview.Source = new BitmapImage(new Uri(_editingMovie.ThumbnailPath));
                    }
                    catch { /* Bỏ qua nếu đường dẫn không hợp lệ */ }
                }

                // 4. Thay đổi nội dung giao diện thành "Sửa"
                PageTitle.Text = "Chỉnh sửa phim";
                SaveBtn.Content = "Cập nhật";
            }
        }

        /// <summary>
        /// Xử lý Lưu (Thêm mới) hoặc Cập nhật (Sửa)
        /// </summary>
        private async void SaveMovie_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra các trường bắt buộc
            if (string.IsNullOrWhiteSpace(TitleInput.Text) || string.IsNullOrWhiteSpace(VideoPathDisplay.Text))
            {
                ShowErrorDialog("Vui lòng nhập tên phim và chọn tệp video.");
                return;
            }

            try
            {
                using var db = new AddDbContext();

                if (_editingMovie != null)
                {
                    // --- CHẾ ĐỘ CẬP NHẬT ---
                    var movieInDb = db.Movies.FirstOrDefault(m => m.Id == _editingMovie.Id);
                    if (movieInDb != null)
                    {
                        // 1. Cập nhật vào Database
                        movieInDb.Title = TitleInput.Text.Trim();
                        movieInDb.Director = DirectorInput.Text?.Trim();
                        movieInDb.Genre = (GenreInput.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Chưa phân loại";
                        movieInDb.Description = DescInput.Text?.Trim();
                        movieInDb.VideoPath = VideoPathDisplay.Text;
                        movieInDb.ThumbnailPath = PosterPathDisplay.Text;

                        db.Movies.Update(movieInDb);

                        // 2. Cập nhật trực tiếp vào đối tượng tham chiếu để trang Detail đổi ngay lập tức
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
                    // --- CHẾ ĐỘ THÊM MỚI ---
                    var newMovie = new Movie
                    {
                        Title = TitleInput.Text.Trim(),
                        Director = DirectorInput.Text?.Trim(),
                        Genre = (GenreInput.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Chưa phân loại",
                        Description = DescInput.Text?.Trim(),
                        VideoPath = VideoPathDisplay.Text,
                        ThumbnailPath = PosterPathDisplay.Text,
                        DateAdded = DateTime.Now
                    };
                    db.Movies.Add(newMovie);
                }

                // Lưu thay đổi xuống file SQLite
                await db.SaveChangesAsync();

                // Quay về trang trước đó
                if (Frame.CanGoBack) Frame.GoBack();
            }
            catch (Exception ex)
            {
                ShowErrorDialog($"Lỗi khi xử lý dữ liệu: {ex.Message}");
            }
        }

        private async void PickVideo_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            var hWnd = WindowNative.GetWindowHandle(App.m_window);
            InitializeWithWindow.Initialize(picker, hWnd);
            picker.ViewMode = PickerViewMode.Thumbnail;
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
                BitmapImage bitmap = new BitmapImage();
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    await bitmap.SetSourceAsync(stream);
                }
                PosterPreview.Source = bitmap;
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