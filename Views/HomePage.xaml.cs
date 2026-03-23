using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.VisualBasic;
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
        public ObservableCollection<Movie> Movies { get; } = new();

        public HomePage()
        {
            this.InitializeComponent();
            InitializeGenrePivot();
            this.Loaded += async (s, e) =>  LoadMoviesFromDb();
        }

        

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

        #region Xử lý Di chuột & Nút chức năng thẻ phim

        private void MovieItem_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Grid rootGrid)
            {
                var overlay = FindChildByName<Border>(rootGrid, "HoverOverlay");
                if (overlay != null)
                {
                    overlay.Opacity = 1;
                }
            }
        }

        private void MovieItem_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Grid rootGrid)
            {
                var overlay = FindChildByName<Border>(rootGrid, "HoverOverlay");
                if (overlay != null)
                {
                    overlay.Opacity = 0;
                }
            }
        }

        private async void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Movie movie)
            {
                // Đã đổi thành AddDbContext
                using var db = new MyMovie.Data.AddDbContext();

                // Đổi trạng thái yêu thích
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

                    if (!Movies.Any())
                    {
                        EmptyState.Visibility = Visibility.Visible;
                        MovieGrid.Visibility = Visibility.Collapsed;
                    }

                    // Đã đổi thành AddDbContext
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

        private List<Movie> _fullMovieList = new List<Movie>();
        public ObservableCollection<Movie> Movie { get; } = new ObservableCollection<Movie>();





        private bool _isPivotInitialized = false;
        //Hàm khởi tạo thanh lọc thể loại
        private void InitializeGenrePivot()
        {
            // 1. Xóa sạch để tránh bị nhân bản Tab khi chuyển trang
            GenreFilterPivot.Items.Clear();

            // 2. Thêm "Tất cả" vào vị trí số 0
            var allItem = new PivotItem
            {
                Header = "Tất cả",
                Tag = "All",
                Content = new Grid() // Tạo nội dung trống để tách biệt các Tab
            };
            GenreFilterPivot.Items.Add(allItem);

            // 3. Duyệt danh sách thể loại từ Constants
            foreach (var genre in MyMovie.Data.Constants.AllGenres)
            {
                var genreItem = new PivotItem
                {
                    Header = genre,
                    Tag = genre,
                    Content = new Grid()
                };
                GenreFilterPivot.Items.Add(genreItem);
            }

            // 4. Cố định: Luôn bắt đầu từ mục đầu tiên
            GenreFilterPivot.SelectedIndex = 0;
        }

        //Hàm thay đổi thể loại, logic hiển thị
        private void GenreFilterPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GenreFilterPivot.SelectedItem is PivotItem selectedTab)
            {
                string filter = selectedTab.Tag?.ToString();

                List<Movie> filteredList;
                Movies.Clear();

                if (filter == "All")
                {
                    filteredList = _fullMovieList; // Hiện tất cả
                }
                else
                {
                    // Lọc chính xác theo chuỗi (Vì cả 2 bên đều dùng chung Constants)
                    filteredList = _fullMovieList.Where(m => m.Genre == filter).ToList();
                }

                foreach (var movie in filteredList)
                {
                    Movies.Add(movie);
                }

                // Xử lý hiện/ẩn thông báo trống
                EmptyState.Visibility = Movies.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        private async void LoadMoviesFromDb()
        {
            using var db = new MyMovie.Data.AddDbContext();
            var list = await db.Movies.OrderByDescending(m => m.DateAdded).ToListAsync();
            _fullMovieList = list;
            Movies.Clear();
            foreach (var movie in list)
            {
                Movies.Add(movie);
            }
            if (Movies.Count > 0)
            {
                // Nếu có phim thì ẨN thông báo trống đi
                EmptyState.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Nếu không có phim thì mới HIỆN thông báo trống
                EmptyState.Visibility = Visibility.Visible;
            }
        }
    }
}