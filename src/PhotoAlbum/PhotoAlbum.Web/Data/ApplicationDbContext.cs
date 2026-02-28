using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PhotoAlbum.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Photo> Photos => Set<Photo>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Photo>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(40);

            entity.Property(p => p.OwnerUserId)
                .IsRequired();

            entity.Property(p => p.BlobName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.UploadedAtUtc)
                .IsRequired();

            entity.HasIndex(p => p.BlobName)
                .IsUnique();

            entity.HasIndex(p => new { p.OwnerUserId, p.Name });

            entity.HasIndex(p => new { p.OwnerUserId, p.UploadedAtUtc });

            entity.HasOne(p => p.OwnerUser)
                .WithMany()
                .HasForeignKey(p => p.OwnerUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
