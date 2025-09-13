using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TurnosPadel.Data;
using TurnosPadel.Enums;
using TurnosPadel.Models;

var builder = WebApplication.CreateBuilder(args);

// Agregar DbContext y conexión a SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddControllersWithViews();

// Autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Login/Logout";
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Crear usuario admin por defecto si no existe
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();

    // Asegurarse de que la base esté creada
    context.Database.EnsureCreated();

    // Verificar si ya existe un admin
    if (!context.Usuarios.Any(u => u.Email == "admin@admin.com"))
    {
        var passwordHasher = new PasswordHasher<Usuario>();

        var admin = new Usuario
        {
            NombreCompleto = "Administrador",
            Email = "admin@admin.com",
            Rol = Rol.Admin,
            numerotel = "0000000000"
        };

        // Hashear correctamente la contraseña "1234"
        admin.ContraseñaHash = passwordHasher.HashPassword(admin, "1234");

        context.Usuarios.Add(admin);
        context.SaveChanges();
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

