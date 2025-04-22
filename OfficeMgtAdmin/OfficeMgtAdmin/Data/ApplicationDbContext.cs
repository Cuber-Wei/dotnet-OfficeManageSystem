using Microsoft.EntityFrameworkCore;
using OfficeMgtAdmin.Models;

namespace OfficeMgtAdmin.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Item> Items { get; set; }
        public DbSet<ImportRecord> ImportRecords { get; set; }
        public DbSet<ApplyRecord> ApplyRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Item>(entity =>
            {
                entity.ToTable("item");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(32);
                entity.Property(e => e.ItemName).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Origin).HasMaxLength(64);
                entity.Property(e => e.ItemSize).HasMaxLength(64);
                entity.Property(e => e.ItemVersion).HasMaxLength(64);
                entity.Property(e => e.ItemPic).HasMaxLength(1024);
            });

            modelBuilder.Entity<ImportRecord>(entity =>
            {
                entity.ToTable("import_record");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Item)
                    .WithMany()
                    .HasForeignKey(e => e.ItemId);
            });

            modelBuilder.Entity<ApplyRecord>(entity =>
            {
                entity.ToTable("apply_record");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Item)
                    .WithMany()
                    .HasForeignKey(e => e.ItemId);
            });
        }
    }
}