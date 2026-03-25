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
        // Liên kết dữ liệu hiển thị với biến toàn cục
        public ObservableCollection<Movie> Movies => App.GlobalHistory;

        public HistoryPage()
        {
            this.InitializeComponent();

            // Cập nhật trạng thái "Trống" khi mở trang và khi danh sách thay đổi
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
                // Sau này sẽ thêm lệnh chuyển sang trang Chi tiết phim (MovieDetailsPage)
            }
        }

        // Handler cho nút xóa khỏi lịch sử (Icon thùng rác)
        private void Unfavorite_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Movie movie)
            {
                // 1. Xóa khỏi danh sách hiển thị
                App.GlobalHistory.Remove(movie);

                // 2. Lưu lại thay đổi vào file JSON ngay lập tức
                MyMovie.Data.HistoryManager.SaveHistory();
            }
        }

        // --- CÁC HÀM XỬ LÝ GIAO DIỆN (HOVER) ---
        private void MovieItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Grid container)
            {
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
        private T? FindControlByName<T>(DependencyObject parent, string name) where T : FrameworkElement
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