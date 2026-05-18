using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Web.Models;

namespace TechMoveGLMS.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Client configuration
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(30);
                entity.Property(e => e.Region).IsRequired().HasMaxLength(100);
            });

            // Contract configuration
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasMaxLength(20);
                entity.Property(e => e.ServiceLevel)
                      .HasConversion<string>()
                      .HasMaxLength(20);

                entity.HasOne(c => c.Client)
                      .WithMany(cl => cl.Contracts)
                      .HasForeignKey(c => c.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ServiceRequest configuration
            modelBuilder.Entity<ServiceRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasMaxLength(20);
                entity.Property(e => e.CostUSD).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CostZAR).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ExchangeRateUsed).HasColumnType("decimal(18,4)");

                entity.HasOne(sr => sr.Contract)
                      .WithMany(c => c.ServiceRequests)
                      .HasForeignKey(sr => sr.ContractId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed data
            modelBuilder.Entity<Client>().HasData(
                new Client
                {
                    Id = 1,
                    Name = "Acme Imports",
                    ContactPerson = "Jane Doe",
                    Email = "jane@acme.co.za",
                    Phone = "+27 11 123 4567",
                    Region = "Gauteng",
                    Address = "10 Sandton Drive, Johannesburg",
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Client
                {
                    Id = 2,
                    Name = "Global Freight SA",
                    ContactPerson = "John Smith",
                    Email = "john@globalfreight.co.za",
                    Phone = "+27 21 987 6543",
                    Region = "Western Cape",
                    Address = "5 Harbour Rd, Cape Town",
                    CreatedAt = new DateTime(2024, 1, 15)
                }
            );
        }
    }
}
