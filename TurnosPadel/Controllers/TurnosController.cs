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

        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Turnos.Include(t => t.Usuario);
            return View(await appDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var turno = await _context.Turnos.Include(t => t.Usuario).FirstOrDefaultAsync(m => m.Id == id);
            if (turno == null) return NotFound();

            return View(turno);
        }

        public IActionResult Create()
        {
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Email");
            return View();
        }

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
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Email", turno.UsuarioId);
            return View(turno);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var turno = await _context.Turnos.FindAsync(id);
            if (turno == null) return NotFound();

            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Email", turno.UsuarioId);
            return View(turno);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Fecha,HoraInicio,Duracion,EsFijo,UsuarioId")] Turno turno)
        {
            if (id != turno.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(turno);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TurnoExists(turno.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Email", turno.UsuarioId);
            return View(turno);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var turno = await _context.Turnos.Include(t => t.Usuario).FirstOrDefaultAsync(m => m.Id == id);
            if (turno == null) return NotFound();

            return View(turno);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var turno = await _context.Turnos.FindAsync(id);
            if (turno != null)
            {
                _context.Turnos.Remove(turno);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Socio,Jugador")]
        public async Task<IActionResult> MisTurnos()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null) return Unauthorized();

            var turnos = await _context.Turnos
                .Where(t => t.UsuarioId == usuario.Id)
                .OrderBy(t => t.Fecha)
                .ThenBy(t => t.HoraInicio)
                .ToListAsync();

            return View(turnos);
        }

        [Authorize(Roles = "Socio,Jugador")]
        public async Task<IActionResult> TurnosLibres(DateTime? fechaSeleccionada)
        {
            // Si no se ha seleccionado una fecha, mostramos los turnos de hoy
            if (!fechaSeleccionada.HasValue)
            {
                fechaSeleccionada = DateTime.Today;
            }

            // Generar turnos para los próximos días si es necesario
            GenerarTurnosParaProximosDias(14);

            // Filtro para mostrar turnos solo del día seleccionado y después de la hora actual con un margen de 30 minutos
            var turnosQuery = _context.Turnos
                .Where(t => t.UsuarioId == null && t.Fecha.Date == fechaSeleccionada.Value.Date
                            && t.Fecha.AddMinutes(t.HoraInicio.Hours * 60 + t.HoraInicio.Minutes) > DateTime.Now.AddMinutes(30));

            var turnos = await turnosQuery
                .OrderBy(t => t.Fecha)
                .ThenBy(t => t.HoraInicio)
                .ToListAsync();

            // Enviar la fecha seleccionada a la vista
            ViewBag.FechaSeleccionada = fechaSeleccionada?.ToString("yyyy-MM-dd");

            return View(turnos);
        }



        // ✅ Reservar turno - RUTA UNICA para evitar AmbiguousMatch
        [HttpPost("Turnos/Reservar/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Socio,Jugador")]
        public async Task<IActionResult> Reservar(int id, [FromForm] bool esFijo)
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

            if (string.IsNullOrWhiteSpace(turno.Cancha))
            {
                TempData["Error"] = "El turno no tiene asignada una cancha.";
                return RedirectToAction(nameof(TurnosLibres));
            }

            turno.UsuarioId = usuario.Id;
            turno.EsFijo = esFijo;

            Console.WriteLine($"[RESERVA] Turno reservado: ID={id}, EsFijo={esFijo}");

            if (esFijo)
            {
                for (int i = 1; i <= 11; i++)
                {
                    Console.WriteLine($"[RESERVA] Creando turno fijo adicional: semana {i}");
                    var nuevoTurno = new Turno
                    {
                        Fecha = turno.Fecha.AddDays(7 * i),
                        HoraInicio = turno.HoraInicio,
                        Duracion = turno.Duracion,
                        Cancha = turno.Cancha,
                        EsFijo = true,
                        UsuarioId = usuario.Id
                    };
                    _context.Turnos.Add(nuevoTurno);
                }
            }

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = esFijo
                ? "Turno fijo reservado para las próximas semanas."
                : "Turno reservado correctamente.";

            return RedirectToAction(nameof(MisTurnos));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Socio,Jugador")]       
        public async Task<IActionResult> CancelarFijo(int id)
        {
            var turno = await _context.Turnos.FindAsync(id);
            if (turno == null)
            {
                TempData["Error"] = "Turno no encontrado.";
                return RedirectToAction("MisTurnos");
            }

            if (!turno.EsFijo)
            {
                TempData["Error"] = "Este turno no es fijo.";
                return RedirectToAction("MisTurnos");
            }

            // Buscar todos los turnos fijos iguales
            var turnosRelacionados = await _context.Turnos
                .Where(t => t.UsuarioId == turno.UsuarioId &&
                            t.Cancha == turno.Cancha &&
                            t.HoraInicio == turno.HoraInicio &&
                            t.EsFijo &&
                            t.Fecha >= DateTime.Today)
                .ToListAsync();

            _context.Turnos.RemoveRange(turnosRelacionados);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Se cancelaron todos los turnos fijos relacionados.";
            return RedirectToAction("MisTurnos");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Socio,Jugador")]
        public async Task<IActionResult> CancelarPorHoy(int id)
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

            // Solo eliminar el turno actual, no los futuros fijos
            turno.UsuarioId = null;

            // Guardar cambios en la base de datos
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Turno cancelado por hoy.";
            return RedirectToAction(nameof(MisTurnos));
        }


        private void GenerarTurnosParaProximosDias(int cantidadDias)
        {
            var hoy = DateTime.Today;

            var horarios = new List<TimeSpan>
            {
                new TimeSpan(14, 0, 0),
                new TimeSpan(15, 30, 0),
                new TimeSpan(17, 0, 0),
                new TimeSpan(18, 30, 0),
                new TimeSpan(20, 0, 0),
                new TimeSpan(21, 30, 0),
                new TimeSpan(23, 0, 0)
            };

            var canchas = new List<string> { "Cancha 1" };

            for (int i = 0; i < cantidadDias; i++)
            {
                var fecha = hoy.AddDays(i);

                foreach (var hora in horarios)
                {
                    foreach (var cancha in canchas)
                    {
                        var existe = _context.Turnos.Any(t =>
                            t.Fecha == fecha &&
                            t.HoraInicio == hora &&
                            t.Cancha == cancha);

                        if (!existe)
                        {
                            _context.Turnos.Add(new Turno
                            {
                                Fecha = fecha,
                                HoraInicio = hora,
                                Duracion = TimeSpan.FromMinutes(90),
                                Cancha = cancha,
                                EsFijo = false
                            });
                        }
                    }
                }
            }

            _context.SaveChanges();
        }

        private bool TurnoExists(int id)
        {
            return _context.Turnos.Any(e => e.Id == id);
        }
    }
}
