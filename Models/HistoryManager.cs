using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using MyMovie.Models;

namespace MyMovie.Data // Đổi namespace nếu bạn để file này ở thư mục khác
{
    public static class HistoryManager
    {
        // Tạo đường dẫn lưu file vào thư mục LocalAppData của Windows
        private static readonly string FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyMovieApp");
        private static readonly string FilePath = Path.Combine(FolderPath, "history.json");

        // Hàm Tải lịch sử khi mở App
        public static void LoadHistory()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    var savedMovies = JsonSerializer.Deserialize<List<Movie>>(json);

                    if (savedMovies != null)
                    {
                        App.GlobalHistory.Clear();
                        foreach (var movie in savedMovies)
                        {
                            App.GlobalHistory.Add(movie);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                System.Diagnostics.Debug.WriteLine("Lỗi khi tải lịch sử: " + ex.Message);
            }
        }

        // Hàm Lưu lịch sử khi có thay đổi (thêm phim, xóa lịch sử)
        public static void SaveHistory()
        {
            try
            {
                if (!Directory.Exists(FolderPath))
                {
                    Directory.CreateDirectory(FolderPath);
                }

                string json = JsonSerializer.Serialize(App.GlobalHistory.ToList());
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khi lưu lịch sử: " + ex.Message);
            }
        }
    }
}