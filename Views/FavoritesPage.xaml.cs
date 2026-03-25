using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic; // Bổ sung để dùng List
using System.Collections.ObjectModel;
using System.Linq;
using MyMovie.Models;
using MyMovie.Data;
using Microsoft.EntityFrameworkCore;
using CommunityToolkit.Mvvm.Messaging; // Bổ sung thư viện nhận tín hiệu

namespace MyMovie.Views
{
    public sealed partial class FavoritesPage : Page
    {
        // 1. Khai báo Property 'Movies' mà XAML đang tìm kiếm
        public ObservableCollection<Movie> Movies { get; set; } = new ObservableCollection<Movie>();

        // THÊM MỚI: Danh sách phụ để lưu trữ dữ liệu gốc, giúp tìm kiếm không bị mất bài
        private List<Movie> _fullFavoriteList = new List<Movie>();

        public FavoritesPage()
        {
            this.InitializeComponent();

            // THÊM MỚI: Lắng nghe tín hiệu gõ chữ Tìm kiếm từ MainPage
            WeakReferenceMessenger.Default.Register<SearchMessage>(this, (r, m) =>
            {
                SearchFavorites(m.Value);
            });

            // THÊM MỚI: Lắng nghe tín hiệu bấm nút Sắp xếp từ MainPage
            WeakReferenceMessenger.Default.Register<SortMessage>(this, (r, m) =>
            {
                SortFavorites(m.Value);
            });
        }

        // 2. Nạp danh sách phim yêu thích từ Database khi mở trang
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LoadFavorites();
        }

        private void LoadFavorites()
        {
            using var db = new AddDbContext();

            // Lấy các phim có IsFavorite = true
            var favoriteMovies = db.Movies.Where(m => m.IsFavorite).ToList();

            // SỬA ĐỔI: Lưu dữ liệu vào danh sách gốc để làm chuẩn
            _fullFavoriteList = favoriteMovies;

            // Đẩy ra màn hình
            UpdateUIList(_fullFavoriteList);
        }

        #region THÊM MỚI: XỬ LÝ TÌM KIẾM VÀ SẮP XẾP

        private void SearchFavorites(string keyword)
        {
            if (_fullFavoriteList == null || !_fullFavoriteList.Any()) return;

            if (string.IsNullOrWhiteSpace(keyword))
            {
                UpdateUIList(_fullFavoriteList); // Xóa trắng ô tìm kiếm thì trả lại toàn bộ
            }
            else
            {
                var searchResult = _fullFavoriteList
                    .Where(m => m.Title != null && m.Title.ToLower().Contains(keyword.ToLower()))
                    .ToList();
                UpdateUIList(searchResult);
            }
        }

        private void SortFavorites(string sortType)
        {
            if (_fullFavoriteList == null || !_fullFavoriteList.Any()) return;

            switch (sortType)
            {
                case "NameAsc":
                    _fullFavoriteList = _fullFavoriteList.OrderBy(m => m.Title).ToList();
                    break;
                case "NameDesc":
                    _fullFavoriteList = _fullFavoriteList.OrderByDescending(m => m.Title).ToList();
                    break;
            }
            UpdateUIList(_fullFavoriteList);
        }

        private void UpdateUIList(List<Movie> listToShow)
        {
            Movies.Clear();
            foreach (var movie in listToShow)
            {
                Movies.Add(movie);
            }
            EmptyState.Visibility = Movies.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region GIỮ NGUYÊN: CÁC XỬ LÝ GIAO DIỆN CŨ

        // 3. Xử lý khi nhấn vào một phim để xem chi tiết
        private void MovieGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Movie selectedMovie)
            {
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

                    // SỬA ĐỔI: Cần xóa luôn khỏi danh sách gốc để tìm kiếm không bị lỗi
                    _fullFavoriteList.Remove(movie);

                    // Kiểm tra lại nếu danh sách trống thì hiện EmptyState
                    if (Movies.Count == 0) EmptyState.Visibility = Visibility.Visible;
                }
            }
        }

        // 5. Các hiệu ứng Hover Overlay
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

        #endregion
    }
}