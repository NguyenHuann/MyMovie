using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using MyMovie.Models;
using MyMovie.Data;
using Microsoft.EntityFrameworkCore;

namespace MyMovie.Views
{
    public sealed partial class FavoritesPage : Page
    {
        // 1. Khai báo Property 'Movies' mà XAML đang tìm kiếm
        // Dùng ObservableCollection để UI tự cập nhật khi xóa phim khỏi danh sách
        public ObservableCollection<Movie> Movies { get; set; } = new ObservableCollection<Movie>();

        public FavoritesPage()
        {
            this.InitializeComponent();
        }

        // 2. Nạp danh sách phim yêu thích từ Database khi mở trang
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LoadFavorites();
        }

        private void LoadFavorites()
        {
            using var db = new AddDbContext(); // Đảm bảo đúng tên Class DbContext của bạn

            // Lấy các phim có IsFavorite = true
            var favoriteMovies = db.Movies.Where(m => m.IsFavorite).ToList();

            Movies.Clear();
            foreach (var movie in favoriteMovies)
            {
                Movies.Add(movie);
            }

            // Hiển thị trạng thái trống nếu không có phim nào
            EmptyState.Visibility = Movies.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        // 3. Xử lý khi nhấn vào một phim để xem chi tiết
        private void MovieGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Movie selectedMovie)
            {
                // Giả định bạn có trang MovieDetailsPage
                Frame.Navigate(typeof(MovieDetailsPage), selectedMovie);
            }
        }

        // 4. Xử lý Bỏ yêu thích (Unfavorite)
        private async void Unfavorite_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Movie movie)
            {
                using var db = new AddDbContext();
                var movieInDb = db.Movies.FirstOrDefault(m => m.Id == movie.Id);

                if (movieInDb != null)
                {
                    // Cập nhật Database
                    movieInDb.IsFavorite = false;
                    await db.SaveChangesAsync();

                    // Xóa khỏi danh sách đang hiển thị trên giao diện
                    Movies.Remove(movie);

                    // Kiểm tra lại nếu danh sách trống thì hiện EmptyState
                    if (Movies.Count == 0) EmptyState.Visibility = Visibility.Visible;
                }
            }
        }

        // 5. Các hiệu ứng Hover Overlay (đã khai báo trong XAML)
        private void MovieItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Grid container)
            {
                var overlay = container.FindName("HoverOverlay") as Border;
                if (overlay != null) overlay.Opacity = 1;
            }
        }

        private void MovieItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Grid container)
            {
                var overlay = container.FindName("HoverOverlay") as Border;
                if (overlay != null) overlay.Opacity = 0;
            }
        }
    }
}