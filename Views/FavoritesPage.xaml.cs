using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyMovie.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyMovie.Views
{
    public sealed partial class FavoritesPage : Page
    {
        public ObservableCollection<Movie> FavoriteMovies { get; } = new();

        public FavoritesPage()
        {
            this.InitializeComponent();
        }

        // Tự động load lại mỗi khi người dùng nhấn vào trang này
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await LoadFavoritesAsync();
        }

        private async Task LoadFavoritesAsync()
        {
            FavoriteMovies.Clear();
            using var db = new MyMovie.Data.AddDbContext();

            // Lọc các phim có IsFavorite = true
            var list = await db.Movies
                               .Where(m => m.IsFavorite == true)
                               .OrderByDescending(m => m.DateAdded)
                               .ToListAsync();

            foreach (var movie in list)
            {
                FavoriteMovies.Add(movie);
            }

            // Hiện thông báo nếu danh sách trống
            EmptyState.Visibility = FavoriteMovies.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        // Khi bấm vào phim để xem chi tiết
        private void MovieGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Movie movie)
            {
                Frame.Navigate(typeof(MovieDetailsPage), movie);
            }
        }

        // Xử lý khi nhấn nút Trái tim ngay tại trang Yêu thích để xóa phim khỏi danh sách
        private async void Unfavorite_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Movie movie)
            {
                using var db = new MyMovie.Data.AddDbContext();

                movie.IsFavorite = false; // Bỏ yêu thích
                db.Movies.Update(movie);
                await db.SaveChangesAsync();

                // Xóa khỏi giao diện ngay lập tức
                FavoriteMovies.Remove(movie);

                // Kiểm tra lại nếu trống thì hiện EmptyState
                if (!FavoriteMovies.Any()) EmptyState.Visibility = Visibility.Visible;
            }
        }
    }
}