using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
        public bool IsFavorite { get; set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}