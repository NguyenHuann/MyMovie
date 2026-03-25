using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using MyMovie.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MyMovie.Views
{
    public sealed partial class HistoryPage : Page
    {
        // Thuộc tính kết nối với danh sách Global
        public ObservableCollection<Movie> HistoryMovies => App.GlobalHistory;

        public HistoryPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Xử lý khi nhấn vào một bộ phim trong lịch sử
        /// </summary>
        private void MovieGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Movie selectedMovie)
            {
                // Điều hướng quay lại trang chi tiết phim hoặc trang phát phim
                // Ví dụ: Frame.Navigate(typeof(MovieDetailsPage), selectedMovie);
            }
        }

        /// <summary>
        /// Xóa sạch danh sách lịch sử
        /// </summary>
        private async void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            // Hiển thị hộp thoại xác nhận (ContentDialog)
            ContentDialog confirmDialog = new ContentDialog
            {
                Title = "Xác nhận xóa",
                Content = "Bạn có chắc chắn muốn xóa toàn bộ lịch sử xem phim không?",
                PrimaryButtonText = "Xóa hết",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot // Quan trọng trong WinUI 3
            };

            ContentDialogResult result = await confirmDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                App.GlobalHistory.Clear();
                // LƯU Ý: Đừng quên gọi hàm HistoryManager.SaveHistory() tại đây nếu bạn có lưu vào file JSON
                // Ví dụ: HistoryManager.SaveHistory();
            }
        }

        // Xử lý khi di chuột vào thẻ
        private void MovieCard_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                // Đổi màu nền sang xám nhạt đồng bộ và sạch sẽ khi di chuột vào (hover)
                border.Background = new SolidColorBrush(Microsoft.UI.Colors.LightGray);
            }
        }

        // Xử lý khi di chuột ra khỏi thẻ
        private void MovieCard_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                // Trả lại nền trong suốt sạch sẽ khi chuột đi ra ngoài
                border.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            }
        }
    }
}