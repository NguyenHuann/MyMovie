using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyMovie.Models;
using System.ComponentModel;

namespace MyMovie.Views
{
    public sealed partial class MovieDetailsPage : Page, INotifyPropertyChanged
    {
        // Đối tượng phim đang được chọn
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

        public event PropertyChangedEventHandler PropertyChanged;
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
        /// Chuyển sang trang PlayerPage và truyền đường dẫn video
        /// </summary>
        private void PlayMovie_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMovie != null && !string.IsNullOrEmpty(SelectedMovie.VideoPath))
            {
                // Điều hướng sang trang phát phim
                Frame.Navigate(typeof(PlayerPage), SelectedMovie.VideoPath);
            }
        }
    }
}