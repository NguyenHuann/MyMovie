using Microsoft.EntityFrameworkCore;
using MyMovie.Models;
using System.IO;
using Windows.Storage;

namespace MyMovie.Data
{
    public class AppDbContext : DbContext
    {
        // Khai báo bảng Movies trong cơ sở dữ liệu
        public DbSet<Movie> Movies { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Thiết lập đường dẫn file SQLite tại thư mục local của ứng dụng
            string dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "movies.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}