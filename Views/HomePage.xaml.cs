using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyMovie.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyMovie.Views
{
    public sealed partial class HomePage : Page
    {
        // Danh sách phim hiển thị trên giao diện
        public ObservableCollection<Movie> Movies { get; } = new();

        // Danh sách gốc dùng để lọc thể loại
        private List<Movie> _fullMovieList = new List<Movie>();

        public HomePage()
        {
            this.InitializeComponent();

            // Đảm bảo trang luôn được tải mới khi quay lại (tránh lỗi cache lịch sử)
            this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Disabled;

            InitializeGenrePivot();
            this.Loaded += async (s, e) => await LoadMoviesFromDb();
        }

        /// <summary>
        /// Logic xử lý khi nhấn vào một bộ phim: Lưu vào lịch sử và chuyển trang
        /// </summary>
        private void MovieGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Movie clickedMovie)
            {
                // 1. Kiểm tra trùng lặp trong kho lưu trữ chung App.GlobalHistory
                var existing = App.GlobalHistory.FirstOrDefault(m => m.Id == clickedMovie.Id);

                if (existing != null)
                {
                    // Nếu đã xem rồi thì xóa bản cũ đi để đưa lên đầu danh sách
                    App.GlobalHistory.Remove(existing);
                }

                // 2. Chèn phim vào vị trí đầu tiên (vị trí 0)
                App.GlobalHistory.Insert(0, clickedMovie);

                // 3. Chuyển sang trang xem chi tiết phim
                Frame.Navigate(typeof(MovieDetailsPage), clickedMovie);
            }
        }

        #region Xử lý Di chuột & Nút chức năng thẻ phim

        private void MovieItem_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Grid rootGrid)
            {
                var overlay = FindChildByName<Border>(rootGrid, "HoverOverlay");
                if (overlay != null) overlay.Opacity = 1;
            }
        }

        private void MovieItem_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Grid rootGrid)
            {
                var overlay = FindChildByName<Border>(rootGrid, "HoverOverlay");
                if (overlay != null) overlay.Opacity = 0;
            }
        }

        private async void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Movie movie)
            {
                using var db = new MyMovie.Data.AddDbContext();

                // Đảo trạng thái yêu thích
                movie.IsFavorite = !movie.IsFavorite;
                db.Movies.Update(movie);
                await db.SaveChangesAsync();

                if (btn.Content is FontIcon icon)
                {
                    if (movie.IsFavorite)
                    {
                        icon.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
                        icon.Glyph = "\xEB52"; // Trái tim đặc
                    }
                    else
                    {
                        icon.ClearValue(FontIcon.ForegroundProperty);
                        icon.Glyph = "\xEB51"; // Trái tim rỗng
                    }
                }
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Movie movie)
            {
                ContentDialog deleteDialog = new ContentDialog
                {
                    Title = "Xác nhận xóa phim",
                    Content = $"Bạn có chắc chắn muốn xóa bộ phim '{movie.Title}' khỏi thư viện không?",
                    PrimaryButtonText = "Xóa",
                    CloseButtonText = "Hủy",
                    XamlRoot = btn.XamlRoot
                };

                var result = await deleteDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    Movies.Remove(movie);
                    _fullMovieList.Remove(movie);

                    if (!Movies.Any())
                    {
                        EmptyState.Visibility = Visibility.Visible;
                    }

                    using var db = new MyMovie.Data.AddDbContext();
                    db.Movies.Remove(movie);
                    await db.SaveChangesAsync();
                }
            }
        }

        private T? FindChildByName<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            int count = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement fe && fe.Name == childName && child is T tChild) return tChild;

                var result = FindChildByName<T>(child, childName);
                if (result != null) return result;
            }
            return null;
        }

        #endregion

        private void InitializeGenrePivot()
        {
            GenreFilterPivot.Items.Clear();
            var allItem = new PivotItem { Header = "Tất cả", Tag = "All", Content = new Grid() };
            GenreFilterPivot.Items.Add(allItem);

            foreach (var genre in MyMovie.Data.Constants.AllGenres)
            {
                GenreFilterPivot.Items.Add(new PivotItem { Header = genre, Tag = genre, Content = new Grid() });
            }
            GenreFilterPivot.SelectedIndex = 0;
        }

        private void GenreFilterPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GenreFilterPivot.SelectedItem is PivotItem selectedTab)
            {
                string filter = selectedTab.Tag?.ToString();
                Movies.Clear();

                var filteredList = (filter == "All")
                    ? _fullMovieList
                    : _fullMovieList.Where(m => m.Genre == filter).ToList();

                foreach (var movie in filteredList) Movies.Add(movie);
                EmptyState.Visibility = Movies.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private async Task LoadMoviesFromDb()
        {
            try
            {
                using var db = new MyMovie.Data.AddDbContext();
                var list = await db.Movies.OrderByDescending(m => m.DateAdded).ToListAsync();
                _fullMovieList = list;

                Movies.Clear();
                foreach (var movie in list) Movies.Add(movie);

                EmptyState.Visibility = Movies.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            catch (Exception) { /* Xử lý lỗi DB nếu cần */ }
        }

        private void AddMovie_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddMoviePage));
        }
    }
}