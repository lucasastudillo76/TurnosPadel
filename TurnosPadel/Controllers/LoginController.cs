using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using TurnosPadel.Data;
using TurnosPadel.Enums;
using TurnosPadel.Models;
using TurnosPadel.ViewModels;



public class LoginController : Controller
{
    private readonly AppDbContext _context;

    public LoginController(AppDbContext context)
    {
        _context = context;
    }

    // =============================
    // REGISTRO
    // =============================
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Registrar()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Registrar(RegistroViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (_context.Usuarios.Any(u => u.Email == model.Email))
        {
            ModelState.AddModelError("Email", "Ya existe un usuario con este email.");
            return View(model);
        }

        var nuevoUsuario = new Usuario
        {
            NombreCompleto = model.NombreCompleto,
            Email = model.Email,
            numerotel = model.numerotel,
            Rol = Rol.Jugador
        };

        var hasher = new PasswordHasher<Usuario>();
        nuevoUsuario.ContraseñaHash = hasher.HashPassword(nuevoUsuario, model.Password);

        _context.Usuarios.Add(nuevoUsuario);
        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "Cuenta creada exitosamente. Iniciá sesión.";
        return RedirectToAction("Index", "Login");
    }

    // =============================
    // LOGIN
    // =============================
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Index(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (usuario == null)
        {
            ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
            return View(model);
        }

        var passwordHasher = new PasswordHasher<Usuario>();
        var result = passwordHasher.VerifyHashedPassword(usuario, usuario.ContraseñaHash, model.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, usuario.NombreCompleto),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Rol.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        if (usuario.Rol == Rol.Admin)
        {
            return RedirectToAction("Index", "Usuarios");
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }

    // =============================
    // LOGOUT
    // =============================
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Login");
    }
}
