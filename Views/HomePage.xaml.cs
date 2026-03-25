using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyMovie.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging; // 1. Bổ sung thư viện nhận tín hiệu

namespace MyMovie.Views
{
    public sealed partial class HomePage : Page
    {
        // Danh sách hiển thị trên giao diện
        public ObservableCollection<Movie> Movies { get; } = new();

        // Danh sách gốc lưu trong bộ nhớ để lọc và tìm kiếm không bị mất bài
        private List<Movie> _fullMovieList = new List<Movie>();

        public HomePage()
        {
            this.InitializeComponent();
            InitializeGenrePivot();

            this.Loaded += (s, e) => LoadMoviesFromDb();

            // 2. Lắng nghe tín hiệu Tìm kiếm từ MainPage
            WeakReferenceMessenger.Default.Register<SearchMessage>(this, (r, m) =>
            {
                SearchMovies(m.Value);
            });

            // 3. Lắng nghe tín hiệu Sắp xếp từ MainPage
            WeakReferenceMessenger.Default.Register<SortMessage>(this, (r, m) =>
            {
                SortMovies(m.Value);
            });
        }

        private async void LoadMoviesFromDb()
        {
            using var db = new MyMovie.Data.AddDbContext();
            var list = await db.Movies.OrderByDescending(m => m.DateAdded).ToListAsync();

            _fullMovieList = list; // Lưu vào danh sách gốc
            ApplyCurrentFilter();  // Áp dụng bộ lọc hiện tại để hiển thị
        }

        #region Logic Tìm kiếm, Sắp xếp & Lọc Thể loại

        // Hàm xử lý khi có tín hiệu Tìm kiếm
        private void SearchMovies(string keyword)
        {
            if (_fullMovieList == null || !_fullMovieList.Any()) return;

            if (string.IsNullOrWhiteSpace(keyword))
            {
                ApplyCurrentFilter(); // Rỗng thì trả về danh sách theo Tab hiện tại
            }
            else
            {
                // Lọc trực tiếp từ danh sách gốc
                var searchResult = _fullMovieList
                    .Where(m => m.Title != null && m.Title.ToLower().Contains(keyword.ToLower()))
                    .ToList();
                UpdateUIList(searchResult);
            }
        }

        // Hàm xử lý khi có tín hiệu Sắp xếp
        private void SortMovies(string sortType)
        {
            if (_fullMovieList == null || !_fullMovieList.Any()) return;

            switch (sortType)
            {
                case "NameAsc":
                    _fullMovieList = _fullMovieList.OrderBy(m => m.Title).ToList();
                    break;
                case "NameDesc":
                    _fullMovieList = _fullMovieList.OrderByDescending(m => m.Title).ToList();
                    break;
            }
            ApplyCurrentFilter(); // Sắp xếp xong thì hiển thị lại theo Tab hiện tại
        }

        // Hàm xác định Tab thể loại đang chọn để hiển thị đúng phim
        private void ApplyCurrentFilter()
        {
            string currentFilter = "All";
            if (GenreFilterPivot.SelectedItem is PivotItem selectedTab)
            {
                currentFilter = selectedTab.Tag?.ToString() ?? "All";
            }

            List<Movie> displayList = currentFilter == "All"
                ? _fullMovieList
                : _fullMovieList.Where(m => m.Genre == currentFilter).ToList();

            UpdateUIList(displayList);
        }

        // Hàm đẩy dữ liệu lên màn hình và kiểm tra trạng thái Rỗng
        private void UpdateUIList(List<Movie> listToShow)
        {
            Movies.Clear();
            foreach (var movie in listToShow)
            {
                Movies.Add(movie);
            }

            EmptyState.Visibility = Movies.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
            MovieGrid.Visibility = Movies.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        // Khởi tạo thanh Thể loại
        private void InitializeGenrePivot()
        {
            GenreFilterPivot.Items.Clear();

            var allItem = new PivotItem { Header = "Tất cả", Tag = "All", Content = new Grid() };
            GenreFilterPivot.Items.Add(allItem);

            foreach (var genre in MyMovie.Data.Constants.AllGenres)
            {
                var genreItem = new PivotItem { Header = genre, Tag = genre, Content = new Grid() };
                GenreFilterPivot.Items.Add(genreItem);
            }

            GenreFilterPivot.SelectedIndex = 0;
        }

        // Khi bấm đổi Tab Thể loại
        private void GenreFilterPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyCurrentFilter();
        }

        #endregion

        #region Xử lý nút chức năng & Hiệu ứng thẻ phim

        private void AddMovie_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddMoviePage));
        }

        private void MovieGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Movie clickedMovie)
            {
                Frame.Navigate(typeof(MovieDetailsPage), clickedMovie);
            }
        }

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
                    _fullMovieList.Remove(movie); // Xóa khỏi danh sách gốc để tìm kiếm không bị lỗi

                    if (!Movies.Any())
                    {
                        EmptyState.Visibility = Visibility.Visible;
                        MovieGrid.Visibility = Visibility.Collapsed;
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
                if (child is FrameworkElement fe && fe.Name == childName && child is T tChild)
                {
                    return tChild;
                }

                var result = FindChildByName<T>(child, childName);
                if (result != null) return result;
            }
            return null;
        }

        #endregion
    }
}