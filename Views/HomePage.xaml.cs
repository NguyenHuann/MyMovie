using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MyMovie.Models;
using MyMovie.Data;
using Microsoft.EntityFrameworkCore;

namespace MyMovie.Views
{
    public sealed partial class HomePage : Page
    {
        // Danh sách phim liên kết với giao diện 
        public ObservableCollection<Movie> Movies { get; } = new();

        public HomePage()
        {
            this.InitializeComponent();
            // Tải dữ liệu ngay khi trang được nạp 
            this.Loaded += async (s, e) => await LoadMoviesFromDbAsync();
        }

        private async Task LoadMoviesFromDbAsync()
        {
            Movies.Clear();

            // Kết nối SQLite để lấy dữ liệu phim 
            using var db = new AppDbContext();
            var movieList = await db.Movies.OrderByDescending(m => m.DateAdded).ToListAsync();

            if (!movieList.Any())
            {
                EmptyState.Visibility = Visibility.Visible;
            }
            else
            {
                EmptyState.Visibility = Visibility.Collapsed;
                foreach (var movie in movieList)
                {
                    Movies.Add(movie);
                }
            }
        }

        private void AddMovie_Click(object sender, RoutedEventArgs e)
        {
            // Điều hướng sang trang thêm phim 
            Frame.Navigate(typeof(AddMoviePage));
        }

        private void MovieGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Chuyển sang trang chi tiết khi nhấn vào phim 
            if (e.ClickedItem is Movie clickedMovie)
            {
                Frame.Navigate(typeof(MovieDetailsPage), clickedMovie);
            }
        }
    }
}