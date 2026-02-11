using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Selu383.SP26.Api.Data;
using Selu383.SP26.Api.Features.Locations;
using Selu383.SP26.Api.Features.Users;
using Selu383.SP26.Api.Features.Roles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext")));

// Add Identity
builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<DataContext>();

// Configure cookies to return 401/403 instead of redirecting
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403;
        return Task.CompletedTask;
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
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

    db.Database.Migrate();

    // Seed roles
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new Role { Name = "Admin" });
    }
    if (!await roleManager.RoleExistsAsync("User"))
    {
        await roleManager.CreateAsync(new Role { Name = "User" });
    }

    // Seed users
    if (await userManager.FindByNameAsync("galkadi") == null)
    {
        var admin = new User { UserName = "galkadi" };
        await userManager.CreateAsync(admin, "Password123!");
        await userManager.AddToRoleAsync(admin, "Admin");
    }

    if (await userManager.FindByNameAsync("bob") == null)
    {
        var bob = new User { UserName = "bob" };
        await userManager.CreateAsync(bob, "Password123!");
        await userManager.AddToRoleAsync(bob, "User");
    }

    if (await userManager.FindByNameAsync("sue") == null)
    {
        var sue = new User { UserName = "sue" };
        await userManager.CreateAsync(sue, "Password123!");
        await userManager.AddToRoleAsync(sue, "User");
    }

    // Seed locations (if needed after migration)
    if (!db.Locations.Any())
    {
        db.Locations.AddRange(
            new Location { Name = "Caffeinated Lions Downtown", Address = "123 Main Street, Hammond, LA 70401", TableCount = 15 },
            new Location { Name = "Caffeinated Lions Uptown", Address = "456 Oak Avenue, Hammond, LA 70403", TableCount = 20 },
            new Location { Name = "Caffeinated Lions Lakeside", Address = "789 Lake Drive, Mandeville, LA 70448", TableCount = 12 }
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

//see: https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0
// Hi 383 - this is added so we can test our web project automatically
public partial class Program { }