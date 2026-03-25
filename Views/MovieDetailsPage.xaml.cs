using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyMovie.Models;
using System.ComponentModel;
using System.Linq; // Thêm thư viện này để dùng được lệnh FirstOrDefault

namespace MyMovie.Views
{
    public sealed partial class MovieDetailsPage : Page, INotifyPropertyChanged
    {
        // Thêm dấu ? để sửa cảnh báo vàng CS8618
        private Movie? _selectedMovie;
        public Movie? SelectedMovie
        {
            get => _selectedMovie;
            set
            {
                _selectedMovie = value;
                OnPropertyChanged(nameof(SelectedMovie));
            }
        }

        // Thêm dấu ? để sửa cảnh báo vàng CS8612
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public MovieDetailsPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Nhận dữ liệu phim từ HomePage truyền sang
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Movie movie)
            {
                SelectedMovie = movie;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack) Frame.GoBack();
        }

        /// <summary>
        /// Chuyển sang trang PlayerPage và TRUYỀN VÀO LỊCH SỬ
        /// </summary>
        private void PlayMovie_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMovie != null && !string.IsNullOrEmpty(SelectedMovie.VideoPath))
            {
                // --- ĐOẠN CODE MỚI: THÊM VÀO LỊCH SỬ XEM PHIM ---
                var existingMovie = App.GlobalHistory.FirstOrDefault(m => m.Id == SelectedMovie.Id);

                if (existingMovie != null)
                {
                    // Nếu đã có trong lịch sử thì xóa vị trí cũ đi
                    App.GlobalHistory.Remove(existingMovie);
                }

                // Cắm bộ phim này lên vị trí trên cùng (mới nhất)
                App.GlobalHistory.Insert(0, SelectedMovie);

                // Gọi quản lý kho lưu lại file JSON lập tức
                MyMovie.Data.HistoryManager.SaveHistory();
                // ----------------------------------------------

                // Điều hướng sang trang phát phim
                Frame.Navigate(typeof(PlayerPage), SelectedMovie.VideoPath);
            }
        }

        private void EditMovie_Click(object sender, RoutedEventArgs e)
        {
            // Điều hướng sang trang AddMoviePage và truyền đối tượng phim hiện tại để sửa
            if (SelectedMovie != null)
            {
                Frame.Navigate(typeof(AddMoviePage), SelectedMovie);
            }
        }

        private void AddMovie_Click(object sender, RoutedEventArgs e)
        {
            // Điều hướng sang trang AddMoviePage để thêm phim mới
            Frame.Navigate(typeof(AddMoviePage));
        }
    }
}