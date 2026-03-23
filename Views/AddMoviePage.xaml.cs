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
using System.Collections.Generic;

namespace MyMovie.Views
{
    public sealed partial class AddMoviePage : Page
    {
        // Lưu trữ đối tượng phim nếu đang ở chế độ Chỉnh sửa
        private Movie _editingMovie = null;

        public AddMoviePage()
        {
            this.InitializeComponent();
            // Đổ dữ liệu từ danh sách string chung
            GenreInput.ItemsSource = Constants.AllGenres;
            GenreInput.SelectedIndex = 0;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Movie movie)
            {
                _editingMovie = movie;

                TitleInput.Text = _editingMovie.Title ?? "";
                DirectorInput.Text = _editingMovie.Director ?? "";
                DescInput.Text = _editingMovie.Description ?? "";
                VideoPathDisplay.Text = _editingMovie.VideoPath ?? "";
                PosterPathDisplay.Text = _editingMovie.ThumbnailPath ?? "";

                // Chọn lại thể loại dựa trên chuỗi string
                if (GenreInput != null && !string.IsNullOrEmpty(_editingMovie.Genre))
                {
                    GenreInput.SelectedItem = _editingMovie.Genre;
                }

                if (!string.IsNullOrEmpty(_editingMovie.ThumbnailPath))
                {
                    await LoadImageAsync(_editingMovie.ThumbnailPath);
                }

                PageTitle.Text = "Chỉnh sửa phim";
                SaveBtn.Content = "Cập nhật";
            }
        }

        // --- HÀM XỬ LÝ LƯU (QUAN TRỌNG) ---
        private async void SaveMovie_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleInput.Text) || string.IsNullOrWhiteSpace(VideoPathDisplay.Text))
            {
                ShowErrorDialog("Vui lòng nhập tên phim và chọn tệp video.");
                return;
            }

            try
            {
                using var db = new AddDbContext();
                string selectedGenre = GenreInput.SelectedItem?.ToString() ?? "Chưa rõ";

                if (_editingMovie != null)
                {
                    var movieInDb = db.Movies.FirstOrDefault(m => m.Id == _editingMovie.Id);
                    if (movieInDb != null)
                    {
                        movieInDb.Title = TitleInput.Text.Trim();
                        movieInDb.Director = DirectorInput.Text?.Trim();
                        movieInDb.Genre = selectedGenre;
                        movieInDb.Description = DescInput.Text?.Trim();
                        movieInDb.VideoPath = VideoPathDisplay.Text;
                        movieInDb.ThumbnailPath = PosterPathDisplay.Text;

                        db.Movies.Update(movieInDb);

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
                    var newMovie = new Movie
                    {
                        Title = TitleInput.Text.Trim(),
                        Director = DirectorInput.Text?.Trim(),
                        Genre = selectedGenre,
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

        // --- CÁC HÀM BỔ TRỢ (FIX LỖI CS0103) ---

        private async Task LoadImageAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(stream);
                    PosterPreview.Source = bitmap;

                    UploadPlaceholder.Visibility = Visibility.Collapsed;
                    ImageSelectedContent.Visibility = Visibility.Visible;
                }
            }
            catch { /* File lỗi hoặc không tồn tại */ }
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
                await LoadImageAsync(file.Path);
            }
        }

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

        private void Back_Click(object sender, RoutedEventArgs e) => Frame.GoBack();
        private void Cancel_Click(object sender, RoutedEventArgs e) => Frame.GoBack();
    }
}