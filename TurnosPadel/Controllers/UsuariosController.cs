using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TurnosPadel.Data;
using TurnosPadel.Enums;
using TurnosPadel.Models;
using System.ComponentModel.DataAnnotations.Schema;
using OfficeOpenXml;

namespace TurnosPadel.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            return View(await _context.Usuarios.ToListAsync());
        }

        // GET: Usuarios/Socios
        public async Task<IActionResult> Socios()
        {
            var socios = await _context.Usuarios
                .Where(u => u.Rol == Rol.Socio)
                .ToListAsync();
            return View(socios);
        }

        // GET: Usuarios/Jugadores
        public async Task<IActionResult> Jugadores()
        {
            var jugadores = await _context.Usuarios
                .Where(u => u.Rol == Rol.Jugador)
                .ToListAsync();
            return View(jugadores);
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombreCompleto,Email,numerotel,ContraseñaHash,Rol")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreCompleto,Email,numerotel,ContraseñaHash,Rol")] Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportarExcel(IFormFile archivoExcel)
        {
            if (archivoExcel == null || archivoExcel.Length == 0)
            {
                TempData["Error"] = "Debes seleccionar un archivo Excel válido.";
                return RedirectToAction("Socios");
            }
            ExcelPackage.License.SetNonCommercialOrganization("<YourNoncommercial Organization>");

            using (var stream = new MemoryStream())
            {

                await archivoExcel.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.First();
                    var rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var nombre = worksheet.Cells[row, 1].Text;
                        var email = worksheet.Cells[row, 2].Text;
                        var telefono = worksheet.Cells[row, 3].Text;
                        

                        // Validar que no exista ya
                        if (_context.Usuarios.Any(u => u.Email == email))
                            continue;

                        var nuevoUsuario = new Usuario
                        {
                            NombreCompleto = nombre,
                            Email = email,
                            numerotel = telefono,
                            Rol = Rol.Socio
                        };

                        // Contraseña por defecto (podés cambiarla a algo más seguro)
                        var password = "Socio123!";

                        var hasher = new PasswordHasher<Usuario>();
                        nuevoUsuario.ContraseñaHash = hasher.HashPassword(nuevoUsuario, password);

                        _context.Usuarios.Add(nuevoUsuario);
                    }

                    await _context.SaveChangesAsync();
                }
            }

            TempData["Mensaje"] = "Importación completada.";
            return RedirectToAction("Socios");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult TurnosReservados()
        {
            var turnos = _context.Turnos
                .Include(t => t.Usuario)
                .Where(t => t.UsuarioId != null)
                .ToList();

            return View("TurnosReservados", turnos);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult TurnosLibres()
        {
            var turnos = _context.Turnos
                .Where(t => t.UsuarioId == null)
                .ToList();

            return View("TurnosLibres", turnos);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ReservarManual(int id)
        {
            var turno = await _context.Turnos.FindAsync(id);
            if (turno == null || turno.UsuarioId != null)
                return NotFound();

            // Aquí podrías redirigir a una vista para elegir usuario, pero por ahora se asigna manualmente:
            turno.UsuarioId = 1; // Por ejemplo, al admin

            await _context.SaveChangesAsync();
            return RedirectToAction("TurnosReservados");
        }


        // GET: Usuarios/AsignarTurno
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> AsignarTurno()
        {
            var turnosLibres = await _context.Turnos
                .Where(t => t.UsuarioId == null)
                .ToListAsync();

            var usuarios = await _context.Usuarios
                .Where(u => u.Rol != Rol.Admin)
                .ToListAsync();

            ViewBag.Turnos = turnosLibres;
            ViewBag.Usuarios = usuarios;

            return View();
        }

        // POST: Usuarios/AsignarTurno
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarTurno(int turnoId, int usuarioId)
        {
            var turno = await _context.Turnos.FindAsync(turnoId);
            var usuario = await _context.Usuarios.FindAsync(usuarioId);

            if (turno == null || usuario == null || turno.UsuarioId != null)
            {
                TempData["Error"] = "Error al asignar el turno. Verificá que el turno esté libre.";
                return RedirectToAction(nameof(AsignarTurno));
            }

            turno.UsuarioId = usuario.Id;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Turno asignado correctamente.";
            return RedirectToAction(nameof(TurnosReservados));
        }



        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }

    }
}

        
