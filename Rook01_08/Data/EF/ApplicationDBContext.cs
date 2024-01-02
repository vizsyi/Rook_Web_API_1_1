using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rook01_08.Models.Auth;
using Rook01_08.Models.Auth.Tokens;

namespace Rook01_08.Data.EF
{
    public class ApplicationDBContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDBContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshToken { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("Auth");

            //ApplicationUser
            modelBuilder.Entity<ApplicationUser>().Property(x => x.UserKey)
                .HasColumnType("char");

            modelBuilder.Entity<ApplicationUser>().Property(x => x.UserName).HasMaxLength(64);
            modelBuilder.Entity<ApplicationUser>().Property(x => x.NormalizedUserName).HasMaxLength(64);

            modelBuilder.Entity<ApplicationUser>().Property(x => x.Email).HasMaxLength(64);
            modelBuilder.Entity<ApplicationUser>().Property(x => x.NormalizedEmail).HasMaxLength(64);

            modelBuilder.Entity<ApplicationUser>().Property(x => x.PasswordHash)
                .HasColumnType("char").HasMaxLength(94);

            modelBuilder.Entity<ApplicationUser>().Property(x => x.SecurityStamp)
                .HasColumnType("char").HasMaxLength(42);

            modelBuilder.Entity<ApplicationUser>().Property(x => x.ConcurrencyStamp)
                .HasColumnType("char").HasMaxLength(36);

            modelBuilder.Entity<ApplicationUser>().Property(x => x.PhoneNumber)
                .HasColumnType("char").HasMaxLength(1);

            //IdentityRole
            modelBuilder.Entity<IdentityRole>().Property(x => x.Name)
                .HasColumnType("varchar").HasMaxLength(32);
            modelBuilder.Entity<IdentityRole>().Property(x => x.NormalizedName)
                .HasColumnType("varchar").HasMaxLength(32);

            //     //RefreshToken
            modelBuilder.Entity<RefreshToken>().Property(x => x.UserKey).HasColumnType("char");

            modelBuilder.Entity<RefreshToken>().Property(x => x.SecKey).HasColumnType("char");

            modelBuilder.Entity<RefreshToken>().Property(x => x.LongToken).HasColumnType("char");

        }
    }
}
