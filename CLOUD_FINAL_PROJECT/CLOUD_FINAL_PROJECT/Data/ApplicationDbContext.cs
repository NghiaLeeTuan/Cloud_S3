using CLOUD_FINAL_PROJECT.Models;
using Microsoft.EntityFrameworkCore;

namespace CLOUD_FINAL_PROJECT.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }
        
        public DbSet<S3FileDetails> S3FileDetails { get; set;}
    }
}
