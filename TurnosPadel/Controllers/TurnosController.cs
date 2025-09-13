using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TurnosPadel.Data;
using TurnosPadel.Models;

namespace TurnosPadel.Controllers
{
    public class TurnosController : Controller
    {
        private readonly AppDbContext _context;

        public TurnosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Turnos
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Turnos.Include(t => t.Usuario);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Turnos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var turno = await _context.Turnos
                .Include(t => t.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (turno == null)
            {
                return NotFound();
            }

            return View(turno);
        }

        // GET: Turnos/Create
        public IActionResult Create()
        {
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "ContraseñaHash");
            return View();
        }

        // POST: Turnos/Create        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Fecha,HoraInicio,Duracion,EsFijo,UsuarioId")] Turno turno)
        {
            if (ModelState.IsValid)
            {
                _context.Add(turno);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "ContraseñaHash", turno.UsuarioId);
            return View(turno);
        }

        // GET: Turnos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var turno = await _context.Turnos.FindAsync(id);
            if (turno == null)
            {
                return NotFound();
            }
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "ContraseñaHash", turno.UsuarioId);
            return View(turno);
        }

        // POST: Turnos/Edit/5       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Fecha,HoraInicio,Duracion,EsFijo,UsuarioId")] Turno turno)
        {
            if (id != turno.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(turno);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TurnoExists(turno.Id))
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
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "ContraseñaHash", turno.UsuarioId);
            return View(turno);
        }

        // GET: Turnos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var turno = await _context.Turnos
                .Include(t => t.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (turno == null)
            {
                return NotFound();
            }

            return View(turno);
        }

        // POST: Turnos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var turno = await _context.Turnos.FindAsync(id);
            if (turno != null)
            {
                _context.Turnos.Remove(turno);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        // 👤 Ver mis turnos reservados
        [Authorize(Roles = "Socio,Jugador")]
        public async Task<IActionResult> MisTurnos()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null) return Unauthorized();

            var turnos = await _context.Turnos
                .Where(t => t.UsuarioId == usuario.Id)
                .OrderBy(t => t.Fecha)
                .ToListAsync();

            return View(turnos);
        }

        // 📅 Ver turnos libres
        [Authorize(Roles = "Socio,Jugador")]
        public async Task<IActionResult> TurnosLibres()
        {
            var turnos = await _context.Turnos
                .Where(t => t.UsuarioId == null)
                .OrderBy(t => t.Fecha)
                .ToListAsync();

            return View(turnos);
        }

        // ✅ Reservar turno
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Socio,Jugador")]
        public async Task<IActionResult> Reservar(int id)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null) return Unauthorized();

            var turno = await _context.Turnos.FindAsync(id);
            if (turno == null || turno.UsuarioId != null)
            {
                TempData["Error"] = "Turno no disponible.";
                return RedirectToAction(nameof(TurnosLibres));
            }

            turno.UsuarioId = usuario.Id;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Turno reservado correctamente.";
            return RedirectToAction(nameof(MisTurnos));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Socio,Jugador")]
        public async Task<IActionResult> ConfirmarReserva(int id)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null) return Unauthorized();

            var turno = await _context.Turnos.FindAsync(id);
            if (turno == null || turno.UsuarioId != null)
            {
                TempData["Error"] = "El turno ya fue reservado.";
                return RedirectToAction(nameof(TurnosLibres));
            }

            turno.UsuarioId = usuario.Id;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Turno reservado exitosamente.";
            return RedirectToAction(nameof(MisTurnos));
        }


        // ❌ Cancelar turno
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Socio,Jugador")]
        public async Task<IActionResult> Cancelar(int id)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null) return Unauthorized();

            var turno = await _context.Turnos.FindAsync(id);
            if (turno == null || turno.UsuarioId != usuario.Id)
            {
                TempData["Error"] = "No puedes cancelar este turno.";
                return RedirectToAction(nameof(MisTurnos));
            }

            turno.UsuarioId = null;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Turno cancelado correctamente.";
            return RedirectToAction(nameof(MisTurnos));
        }    


        private bool TurnoExists(int id)
        {
            return _context.Turnos.Any(e => e.Id == id);
        }
    }
}
