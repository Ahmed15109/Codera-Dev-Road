using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using progect_DEPI.Models;
using Rotativa.AspNetCore;

namespace progect_DEPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured before starting the application.");
            }

            // Add services to the container.
            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
            });

            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            //.AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var roles = new[] { "Admin", "User" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var bootstrapAdminEnabled = builder.Configuration.GetValue<bool>("BootstrapAdmin:Enabled");

            if (bootstrapAdminEnabled)
            {
                using var scope = app.Services.CreateScope();
                await EnsureBootstrapAdminAsync(scope.ServiceProvider, builder.Configuration);
            }

            app.Run();
        }

        private static async Task EnsureBootstrapAdminAsync(IServiceProvider services, IConfiguration configuration)
        {
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var dbContext = services.GetRequiredService<ApplicationDbContext>();
            var email = configuration["BootstrapAdmin:Email"];
            var fullName = configuration["BootstrapAdmin:FullName"];
            var password = configuration["BootstrapAdmin:Password"];

            if (string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(fullName)
                || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("BootstrapAdmin:Email, BootstrapAdmin:FullName and BootstrapAdmin:Password are required when BootstrapAdmin:Enabled is true.");
            }

            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var identityUser = await userManager.FindByEmailAsync(email);
                if (identityUser == null)
                {
                    identityUser = new IdentityUser
                    {
                        UserName = email,
                        Email = email
                    };

                    var createResult = await userManager.CreateAsync(identityUser, password);
                    if (!createResult.Succeeded)
                    {
                        throw new InvalidOperationException("Bootstrap administrator creation failed.");
                    }
                }

                var linkedProfiles = await dbContext.Users
                    .Where(user => user.IdentityId == identityUser.Id)
                    .ToListAsync();

                if (linkedProfiles.Count > 1)
                {
                    throw new InvalidOperationException("Bootstrap administrator has duplicate domain profiles.");
                }

                var domainUser = linkedProfiles.SingleOrDefault();
                if (domainUser == null)
                {
                    var matchingProfiles = await dbContext.Users
                        .Where(user => user.Email == email)
                        .ToListAsync();

                    if (matchingProfiles.Count > 1)
                    {
                        throw new InvalidOperationException("Bootstrap administrator has duplicate email profiles.");
                    }

                    domainUser = matchingProfiles.SingleOrDefault();
                    if (domainUser != null)
                    {
                        if (!string.IsNullOrWhiteSpace(domainUser.IdentityId)
                            && domainUser.IdentityId != identityUser.Id)
                        {
                            throw new InvalidOperationException("Bootstrap administrator email is already linked to another profile.");
                        }

                        domainUser.IdentityId = identityUser.Id;
                    }
                    else
                    {
                        domainUser = new User
                        {
                            FullName = fullName,
                            Email = email,
                            Picture = null,
                            CreatedAt = DateTime.Now,
                            UpdateAt = DateTime.Now,
                            IdentityId = identityUser.Id
                        };

                        dbContext.Users.Add(domainUser);
                    }
                }

                if (!await userManager.IsInRoleAsync(identityUser, "Admin"))
                {
                    var roleResult = await userManager.AddToRoleAsync(identityUser, "Admin");
                    if (!roleResult.Succeeded)
                    {
                        throw new InvalidOperationException("Bootstrap administrator role assignment failed.");
                    }
                }

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
