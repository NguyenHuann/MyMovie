using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyMovie.Models;
using System;
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
            this.Loaded += async (s, e) => await LoadMoviesFromDbAsync();
        }

        private async Task LoadMoviesFromDbAsync()
        {
            Movies.Clear();

            // Đã đổi thành AddDbContext cho chuẩn với máy của bạn
            using var db = new MyMovie.Data.AddDbContext();
            var movieList = await db.Movies.OrderByDescending(m => m.DateAdded).ToListAsync();

            if (!movieList.Any())
            {
                EmptyState.Visibility = Visibility.Visible;
                MovieGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyState.Visibility = Visibility.Collapsed;
                MovieGrid.Visibility = Visibility.Visible;

                foreach (var movie in movieList)
                {
                    Movies.Add(movie);
                }
            }
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
    }
}