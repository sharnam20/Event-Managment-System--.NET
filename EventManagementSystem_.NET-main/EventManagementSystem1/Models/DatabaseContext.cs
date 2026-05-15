using System;
using System.Data.Entity;
using System.Linq;

namespace EventManagementSystem1.Models
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext() : base("name=DefaultConnection")
        {
            // Enable database creation if it doesn't exist
            Database.SetInitializer(new CreateDatabaseIfNotExists<DatabaseContext>());

            // Enable lazy loading
            Configuration.LazyLoadingEnabled = true;
            Configuration.ProxyCreationEnabled = true;
        }

        public void InitializeDatabase()
        {
            try
            {
                // Force database creation if it doesn't exist
                Database.Initialize(force: false);

                // Create default admin user if no users exist
                if (!Users.Any())
                {
                    var adminUser = new User
                    {
                        Username = "admin",
                        Password = "admin123",
                        Email = "admin@eventmanagement.com",
                        FirstName = "Admin",
                        LastName = "User",
                        Role = "Admin",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };
                    Users.Add(adminUser);
                    SaveChanges();
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't throw to avoid breaking the application
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
            }
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Configure relationships
            modelBuilder.Entity<Event>()
                .HasOptional(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId);

            modelBuilder.Entity<Event>()
                .HasRequired(e => e.Venue)
                .WithMany()
                .HasForeignKey(e => e.VenueId);

            modelBuilder.Entity<Event>()
                .HasRequired(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy);

            modelBuilder.Entity<Participant>()
                .HasRequired(p => p.Event)
                .WithMany()
                .HasForeignKey(p => p.EventId);

            modelBuilder.Entity<Participant>()
                .HasRequired(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<Ticket>()
                .HasRequired(t => t.Event)
                .WithMany()
                .HasForeignKey(t => t.EventId);

            modelBuilder.Entity<Ticket>()
                .HasRequired(t => t.Participant)
                .WithMany()
                .HasForeignKey(t => t.ParticipantId);

            // Configure unique constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.TicketNumber)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}