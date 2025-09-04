using Microsoft.EntityFrameworkCore;
using Attribute.Api.Models;

namespace Attribute.Api.Data
{
    public class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
    {
        public DbSet<AppUser> Users => Set<AppUser>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<UserRoleScope> UserRoleScopes => Set<UserRoleScope>();
        public DbSet<Region> Regions => Set<Region>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<AppResource> Resources => Set<AppResource>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("uuid-ossp");
            
            // Configure primary keys
            modelBuilder.Entity<UserRoleScope>().HasKey(x => new { x.UserId, x.RoleId, x.Scope, x.RegionId, x.LocationId });
            modelBuilder.Entity<RolePermission>().HasKey(x => new { x.RoleId, x.PermissionId });
            
            // Configure table names
            modelBuilder.Entity<AppUser>().ToTable("app_user");
            modelBuilder.Entity<AppUser>().Property(u => u.Id).HasColumnName("id");
            modelBuilder.Entity<AppUser>().Property(u => u.Username).HasColumnName("username");
            modelBuilder.Entity<AppUser>().Property(u => u.PasswordHash).HasColumnName("password_hash");
            modelBuilder.Entity<AppUser>().Property(u => u.DisplayName).HasColumnName("display_name");
            
            modelBuilder.Entity<Role>().ToTable("role");
            modelBuilder.Entity<Role>().Property(r => r.Id).HasColumnName("id");
            modelBuilder.Entity<Role>().Property(r => r.Name).HasColumnName("name");
            modelBuilder.Entity<Role>().Property(r => r.Description).HasColumnName("description");
            modelBuilder.Entity<Role>().Property(r => r.ParentRoleId).HasColumnName("parent_role_id");
            
            modelBuilder.Entity<Permission>().ToTable("permission");
            modelBuilder.Entity<Permission>().Property(p => p.Id).HasColumnName("id");
            modelBuilder.Entity<Permission>().Property(p => p.Resource).HasColumnName("resource");
            modelBuilder.Entity<Permission>().Property(p => p.Action).HasColumnName("action");
            
            modelBuilder.Entity<RolePermission>().ToTable("role_permission");
            modelBuilder.Entity<RolePermission>().Property(rp => rp.RoleId).HasColumnName("role_id");
            modelBuilder.Entity<RolePermission>().Property(rp => rp.PermissionId).HasColumnName("permission_id");
            
            modelBuilder.Entity<UserRoleScope>().ToTable("user_role_scope");
            modelBuilder.Entity<UserRoleScope>().Property(urs => urs.UserId).HasColumnName("user_id");
            modelBuilder.Entity<UserRoleScope>().Property(urs => urs.RoleId).HasColumnName("role_id");
            modelBuilder.Entity<UserRoleScope>().Property(urs => urs.Scope).HasColumnName("scope");
            modelBuilder.Entity<UserRoleScope>().Property(urs => urs.RegionId).HasColumnName("region_id");
            modelBuilder.Entity<UserRoleScope>().Property(urs => urs.LocationId).HasColumnName("location_id");
            
            modelBuilder.Entity<Region>().ToTable("region");
            modelBuilder.Entity<Region>().Property(r => r.Id).HasColumnName("id");
            modelBuilder.Entity<Region>().Property(r => r.Code).HasColumnName("code");
            modelBuilder.Entity<Region>().Property(r => r.Name).HasColumnName("name");
            
            modelBuilder.Entity<Location>().ToTable("location");
            modelBuilder.Entity<Location>().Property(l => l.Id).HasColumnName("id");
            modelBuilder.Entity<Location>().Property(l => l.Code).HasColumnName("code");
            modelBuilder.Entity<Location>().Property(l => l.Name).HasColumnName("name");
            modelBuilder.Entity<Location>().Property(l => l.RegionId).HasColumnName("region_id");
            
            modelBuilder.Entity<AppResource>().ToTable("app_resource");
            modelBuilder.Entity<AppResource>().Property(ar => ar.Id).HasColumnName("id");
            modelBuilder.Entity<AppResource>().Property(ar => ar.Title).HasColumnName("title");
            modelBuilder.Entity<AppResource>().Property(ar => ar.OwnerId).HasColumnName("owner_id");
            modelBuilder.Entity<AppResource>().Property(ar => ar.RegionId).HasColumnName("region_id");
            modelBuilder.Entity<AppResource>().Property(ar => ar.LocationId).HasColumnName("location_id");
        }
    }
}