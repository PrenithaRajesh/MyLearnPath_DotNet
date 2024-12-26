using Microsoft.EntityFrameworkCore;
using DBConnect.Models;
using Microsoft.Extensions.Configuration;

namespace DBConnect.Data{
    public class DataContextEf : DbContext{
        public DbSet<Computer>? Computer { get; set; }
        private string _connectionString;
        
        public DataContextEf(IConfiguration configuration){
            // _configuration = configuration;
            _connectionString = configuration?.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder){
            if(!optionsBuilder.IsConfigured){
                optionsBuilder.UseSqlServer(_connectionString,
                options => options.EnableRetryOnFailure());
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("TutorialAppSchema");

            modelBuilder.Entity<Computer>().HasKey(c => c.ComputerId);
            //.HasNoKey();
            //.ToTable("Computer", "TutorialAppSchema");
            //.ToTable("Table_Name", "Schema_Name");
        }

    }
}