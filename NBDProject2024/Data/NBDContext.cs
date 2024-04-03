using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using NBDProject2024.Models;

namespace NBDProject2024.Data
{
    public class NBDContext : DbContext
    {
        //To give access to IHttContextAceessor for Audit Data With IAuditable
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Property on hold the UserName value
        public string UserName
        {
            get; private set;
        }

        public NBDContext(DbContextOptions<NBDContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            if (_httpContextAccessor.HttpContext != null)
            {
                //We have a HttpContext, but there might not be anyone Authenticated
                UserName = _httpContextAccessor.HttpContext?.User.Identity.Name;
                UserName ??= "Unknown";
            }
            else
            {
                //No HttpContext so seeding data
                UserName = "Seed Data";
            }
        }
        public NBDContext(DbContextOptions<NBDContext> options) : base(options)
        {
        }

        public DbSet<Bid> Bids { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<BidLabour> BidLabours { get; set; }
        public DbSet<Labour> Labours { get; set; }
        public DbSet<BidMaterial> BidMaterials { get; set; }
        public DbSet<Material> Materials { get; set; }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Project> Projects { get; set; }

        public DbSet<Province> Provinces { get; set; }
        public DbSet<City> Cities { get; set; }
       



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                 .HasIndex(e => new { e.Email })
                 .IsUnique();

            modelBuilder.Entity<Client>()
                .HasIndex(c  => new { c.Email })
                .IsUnique();


            //Add a unique index to the City/Province
            modelBuilder.Entity<City>()
            .HasIndex(c => new { c.Name, c.ProvinceID })
            .IsUnique();

            //Prevent Cascade Delete
            modelBuilder.Entity<Province>()
                .HasMany<City>(d => d.Cities)
                .WithOne(p => p.Province)
                .HasForeignKey(p => p.ProvinceID)
                .OnDelete(DeleteBehavior.Restrict);

            //Add a unique index to Material Name
            modelBuilder.Entity<Material>()
                .HasIndex(c => c.Name)
                .IsUnique();

            //Add a unique index to Labour Name
            modelBuilder.Entity<Labour>()
                .HasIndex(c => c.Name)
                .IsUnique();

            //Prevent cascade delete from materials and
            //labors to bidMaterial and BidLabour
            modelBuilder.Entity<Material>()
                .HasMany<BidMaterial>(m => m.BidMaterials)
                .WithOne(i => i.Materials)
                .HasForeignKey(i => i.MaterialID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Labour>()
                .HasMany<BidLabour>(l => l.BidLabours)
                .WithOne(i => i.Labours)
                .HasForeignKey(l => l.LabourID)
                .OnDelete(DeleteBehavior.Restrict);


            //Many to Many Intersection
            //modelBuilder.Entity<Position>()
                //.HasKey(t => new { t.ProjectID, t.StaffID });

          
            //Prevent cascade delete from staff top position
            //so we are prevented from deleting a staff with 
            //projects they have worked
          /*  //modelBuilder.Entity<Staff>()
                **.HasMany<Position>(s => s.Positions)
                .WithOne(p => p.Staff)
                .HasForeignKey(p => p.StaffID)
                .OnDelete(DeleteBehavior.Restrict);
          */
       

            //Prevent Cascade Delete from Client to ClientProjects
            //So we are preventedd from deleing a Client with a Project
                 modelBuilder.Entity<Client>()
                .HasMany<Project>(cp => cp.Projects)
                .WithOne(p => p.Client)
                .HasForeignKey(p => p.ClientID)
                .OnDelete(DeleteBehavior.Restrict);


            //Prevent Cascade Delete from   Projects to Bid
            //So we are preventedd from deleing a Project with a Bid
            modelBuilder.Entity<Project>()
               .HasMany<Bid>(p => p.Bids)
               .WithOne(b => b.Project)
               .HasForeignKey(b => b.ProjectID)
               .OnDelete(DeleteBehavior.Restrict);



        }
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.Entity is IAuditable trackable)
                {
                    var now = DateTime.UtcNow;
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;

                        case EntityState.Added:
                            trackable.CreatedOn = now;
                            trackable.CreatedBy = UserName;
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;
                    }
                }
            }
        }
    }
}
