using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using MyMovie.Models;
using System;
using System.Collections.ObjectModel;

namespace MyMovie.Views
{
    public sealed partial class HistoryPage : Page
    {
        // Sửa tên thành 'Movies' để khớp chính xác với {x:Bind Movies} trong XAML
        public ObservableCollection<Movie> Movies => App.GlobalHistory;

        public HistoryPage()
        {
            this.InitializeComponent();
            // Kiểm tra hiển thị trạng thái trống
            UpdateEmptyState();
            Movies.CollectionChanged += (s, e) => UpdateEmptyState();
        }

        private void UpdateEmptyState()
        {
            EmptyState.Visibility = (Movies == null || Movies.Count == 0)
                                    ? Visibility.Visible
                                    : Visibility.Collapsed;
        }

        private void MovieGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Movie selectedMovie)
            {
                // Logic điều hướng xem phim
            }
        }

        // Handler cho nút "Bỏ yêu thích" (Icon trái tim)
        private void Unfavorite_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Movie movie)
            {
                App.GlobalHistory.Remove(movie);
                // Lưu lại file nếu cần: HistoryManager.SaveHistory();
            }
        }

        // Hiệu ứng Hover cho Item
        private void MovieItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Grid container)
            {
                // Tìm Border có tên HoverOverlay trong DataTemplate
                var overlay = FindControlByName<Border>(container, "HoverOverlay");
                if (overlay != null) overlay.Opacity = 1;
            }
        }

        private void MovieItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Grid container)
            {
                var overlay = FindControlByName<Border>(container, "HoverOverlay");
                if (overlay != null) overlay.Opacity = 0;
            }
        }

        // Hàm phụ trợ để tìm control bên trong DataTemplate
        private T FindControlByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T element && element.Name == name) return element;
                var result = FindControlByName<T>(child, name);
                if (result != null) return result;
            }
            return null;
        }
    }
}