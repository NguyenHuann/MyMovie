using System;

namespace MyMovie.Models
{
    public class Movie
    {
        public int Id { get; set; }

        // Khởi tạo giá trị rỗng mặc định để tránh lỗi null
        public string Title { get; set; } = string.Empty;

        // Hoặc cho phép null nếu trường đó không bắt buộc
        public string? Director { get; set; }
        public string? Genre { get; set; }
        public string? Description { get; set; }
        public string? VideoPath { get; set; }
        public string? ThumbnailPath { get; set; }

        public DateTime DateAdded { get; set; }
    }
}