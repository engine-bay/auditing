namespace EngineBay.Auditing.Tests
{
    using EngineBay.Persistence;
    using Humanizer;
    using Microsoft.EntityFrameworkCore;

    public class FakeModel : AuditableModel
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public static new void CreateDataAnnotations(ModelBuilder modelBuilder)
        {
            if (modelBuilder is null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Entity<FakeModel>().ToTable(typeof(FakeModel).Name.Pluralize());

            modelBuilder.Entity<FakeModel>().HasKey(x => x.Id);

            modelBuilder.Entity<FakeModel>().Property(x => x.CreatedAt).IsRequired();

            modelBuilder.Entity<FakeModel>().Property(x => x.LastUpdatedAt).IsRequired();

            modelBuilder.Entity<FakeModel>().Property(x => x.CreatedById).IsRequired();

            modelBuilder.Entity<FakeModel>().HasOne(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FakeModel>().Property(x => x.LastUpdatedById).IsRequired();

            modelBuilder.Entity<FakeModel>().HasOne(x => x.LastUpdatedBy).WithMany().HasForeignKey(x => x.LastUpdatedById).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FakeModel>().Property(x => x.Name).IsRequired();

            modelBuilder.Entity<FakeModel>().Property(x => x.Description).IsRequired();
        }
    }
}
