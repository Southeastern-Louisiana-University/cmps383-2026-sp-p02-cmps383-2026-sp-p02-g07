using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Selu383.SP26.Api.Data;
using Selu383.SP26.Api.Features.Locations;
using Selu383.SP26.Api.Features.Roles;
using Selu383.SP26.Api.Features.Users;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext")));

// Identity services with EF stores
builder.Services
    .AddIdentity<User, Role>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = false;
        options.User.AllowedUserNameCharacters = null;
    })
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

// Configure cookie to return 401/403 for API (no redirects)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/api"))
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
            ctx.Response.Redirect(ctx.RedirectUri);
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/api"))
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
            ctx.Response.Redirect(ctx.RedirectUri);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    db.Database.MigrateAsync();

    // Seed roles and users
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    async Task EnsureRoleAsync(string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new Role { Name = roleName });
        }
    }

    async Task<User> EnsureUserAsync(string userName, string password)
    {
        var user = await userManager.FindByNameAsync(userName);
        if (user == null)
        {
            user = new User { UserName = userName };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new Exception("Failed to create seed user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        return user;
    }

    await EnsureRoleAsync("Admin");
    await EnsureRoleAsync("User");

    var admin = await EnsureUserAsync("galkadi", "Password123!");
    var bob = await EnsureUserAsync("bob", "Password123!");
    var sue = await EnsureUserAsync("sue", "Password123!");

    async Task EnsureInRole(User u, string role)
    {
        if (!await userManager.IsInRoleAsync(u, role))
        {
            await userManager.AddToRoleAsync(u, role);
        }
    }

    await EnsureInRole(admin, "Admin");
    await EnsureInRole(bob, "User");
    await EnsureInRole(sue, "User");

    if (!db.Locations.Any())
    {
        db.Locations.AddRange(
            new Location { Name = "Location 1", Address = "123 Main St", TableCount = 10 },
            new Location { Name = "Location 2", Address = "456 Oak Ave", TableCount = 20 },
            new Location { Name = "Location 3", Address = "789 Pine Ln", TableCount = 15 }
        );
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization()
    .UseEndpoints(x =>
    {
        x.MapControllers();
    });

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSpa(x =>
    {
        x.UseProxyToSpaDevelopmentServer("http://localhost:5173");
    });
}
else
{
    app.MapFallbackToFile("/index.html");
}



app.Run();

//see: https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0
// Hi 383 - this is added so we can test our web project automatically
public partial class Program { }