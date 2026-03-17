using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Runtime.CompilerServices;

namespace MyMovie.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Models.Movie> Movies { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Lấy đường dẫn tuyệt đối của file AppDbContext.cs lúc biên dịch
            string sourceFilePath = GetSourceFilePath();

            // Lấy thư mục chứa file này (chính là thư mục Data của dự án)
            string dataFolderPath = Path.GetDirectoryName(sourceFilePath) ?? "";

            // Đường dẫn đến file db ngay trong thư mục Data
            string dbPath = Path.Combine(dataFolderPath, "movies.db");

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        // Hàm hỗ trợ lấy đường dẫn file nguồn
        private static string GetSourceFilePath([CallerFilePath] string? path = null) => path ?? "";
    }
}