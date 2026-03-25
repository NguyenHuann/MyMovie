using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Messaging.Messages; // 1. Đã thêm thư viện này ở đầu file

namespace MyMovie.Models
{
    public class Movie : INotifyPropertyChanged
    {
        public int Id { get; set; }

        private string? _title;
        public string? Title { get => _title; set { _title = value; OnPropertyChanged(); } }

        private string? _director;
        public string? Director { get => _director; set { _director = value; OnPropertyChanged(); } }

        private string? _genre;
        public string? Genre { get => _genre; set { _genre = value; OnPropertyChanged(); } }

        private string? _description;
        public string? Description { get => _description; set { _description = value; OnPropertyChanged(); } }

        public string? VideoPath { get; set; }
        public string? ThumbnailPath { get; set; }
        public DateTime DateAdded { get; set; }

        private bool _isFavorite;
        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                _isFavorite = value;
                OnPropertyChanged(nameof(IsFavorite));
            }
        }

        // Đã thêm dấu "?" để tránh cảnh báo vàng CS8612
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // ==========================================
    // 2. ĐÃ THÊM 2 CLASS TÍN HIỆU Ở NGOÀI CLASS MOVIE
    // ==========================================
    public class SortMessage : ValueChangedMessage<string>
    {
        public SortMessage(string value) : base(value) { }
    }

    public class SearchMessage : ValueChangedMessage<string>
    {
        public SearchMessage(string value) : base(value) { }
    }
}