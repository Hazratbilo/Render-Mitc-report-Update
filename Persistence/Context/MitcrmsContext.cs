using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MITCRMS.Models.Entities;

namespace MITCRMS.Persistence.Context
{

        public class MitcrmsContext : DbContext
        {
            public MitcrmsContext(DbContextOptions<MitcrmsContext> options) : base(options)
            {

            }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                //builder.Ignore<Microsoft.AspNetCore.Mvc.Rendering.SelectListGroup>();
                //builder.Ignore<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
                SeedSuperAdminData(builder);


                    SeedRoleData(builder);


                            builder.Entity<UserRole>()
                                .HasOne(ur => ur.User)
                                .WithMany(u => u.UserRoles)
                                .HasForeignKey(ur => ur.UserId);

                            builder.Entity<UserRole>()
                                .HasOne(ur => ur.Role)
                                .WithMany(r => r.UserRoles)
                                .HasForeignKey(ur => ur.RoleId);

                            builder.Entity<User>()
                            .HasIndex(u => u.Email)
                            .IsUnique();

                    builder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId);

                builder.Entity<User>()
                .HasMany(u => u.Reports)
                .WithOne(r => r.User)
                .OnDelete(DeleteBehavior.Cascade);

                    //base.OnModelCreating(builder);
            }

            private static void SeedSuperAdminData(ModelBuilder modelBuilder)
            {

                var SuperadminRoleId = new Guid("d2719e67-52f4-4f9c-bdb2-123456789abc");
                var SuperadminUserId = new Guid("c8f2e5ab-9f34-4b97-8b7c-1a5e86c77e42");
                var SuperadminDeptId = new Guid("c8f2e7bb-9f34-4b97-8b7c-1a5e46c77e42");

                var role = new Role
                    {
                        Id = SuperadminRoleId,
                        RoleName = "SuperAdmin",
                
                        DateCreated = DateTime.SpecifyKind(new DateTime(2026, 1, 10), DateTimeKind.Utc)
                };

                var superAdminDept = new Department
                {
                    Id = SuperadminDeptId,
                    DepartmentName = "Director of Studies",
                    DepartmentCode = "D.O.S-001"
                };

            

            var hasher = new PasswordHasher<object>();
                var passwordHash = hasher.HashPassword(null, "Admin@001");

                var SuperadminProfile = new User
                {
                    Id =SuperadminUserId,
                    Email = "Admin001@gmail.com",
                    DepartmentId = superAdminDept.Id,
                    PasswordHash = "AQAAAAIAAYagAAAAEJjieFsJGM2Xgr+WpuS3juOABbBCvbqSvpym4WzP/SDMuvGz6qH+EFgm19l8SUHUGA==",
                    FirstName = "SuperAdmin",
                    LastName = "Mitcrms",
                    Address = "Ogun State",
                    PhoneNumber = "+23470456780",
                    EmailConfirmed = true,
                    DateCreated = DateTime.SpecifyKind(new DateTime(2025, 11, 10), DateTimeKind.Utc),
                };


            var userRole = new UserRole
                {
                    Id = new Guid("7ad9b1e1-4c23-46a2-b8e4-219ab417f71f"),
                    RoleId = SuperadminRoleId,
                    UserId = SuperadminUserId,
                    DateCreated = DateTime.SpecifyKind(new DateTime(2026, 1, 10), DateTimeKind.Utc)
                };

                

                modelBuilder.Entity<Role>().HasData(role);
            modelBuilder.Entity<Department>().HasData(superAdminDept);
            modelBuilder.Entity<User>().HasData(SuperadminProfile);
                modelBuilder.Entity<UserRole>().HasData(userRole);
               
        }

            private void SeedRoleData(ModelBuilder modelBuilder)
            {
                var roles = new List<Role>
            {
                new Role
                {
                    Id = new Guid("a45c9e02-1f0b-4e57-b3d8-9b77b4a302be"),
                    RoleName = "Hod",
                    DateCreated = DateTime.SpecifyKind(new DateTime(2026, 1, 10), DateTimeKind.Utc),
                },
                new Role
                {
                    Id = new Guid("6e3d4978-dcb0-42ea-9c48-7f6209d4a871"),
                    RoleName = "Bursar",
                    DateCreated = DateTime.SpecifyKind(new DateTime(2026, 1, 10), DateTimeKind.Utc),
                },
                       new Role
                {
                    Id = new Guid("6e3d4978-dcb0-42ea-9c48-7f6209d4a882"),
                    RoleName = "Admin",
                    DateCreated = DateTime.SpecifyKind(new DateTime(2026, 1, 10), DateTimeKind.Utc),
                },
                              new Role
                {
                    Id = new Guid("6e3d4978-dcb0-42ea-9c48-7f6209d4a856"),
                    RoleName = "Tutor",
                    DateCreated = DateTime.SpecifyKind(new DateTime(2026, 1, 10), DateTimeKind.Utc),
                }
            };

                modelBuilder.Entity<Role>().HasData(roles);
        }

            DbSet<User> Users => Set<User>();

            DbSet<UserRole> UserRoles => Set<UserRole>();
            DbSet<Department> Departments => Set<Department>();
            DbSet<Report> Reports => Set<Report>();
        DbSet<Role> Roles => Set<Role>();
        }
}
