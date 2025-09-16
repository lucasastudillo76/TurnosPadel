using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using MercadoPago.Config;
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Preference;
using MercadoPago.Client.Payment;
using MercadoPago.Resource.Payment;
using TurnosPadel.Data;
using TurnosPadel.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TurnosPadel.Controllers
{
    public class PagoController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public PagoController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        // ✅ Paso 1: Crear la preferencia con referencia al turno
        public async Task<IActionResult> CrearPago(int turnoId)
        {
            var turno = await _context.Turnos.FindAsync(turnoId);
            if (turno == null)
            {
                TempData["Error"] = "Turno no encontrado.";
                return RedirectToAction("MisTurnos", "Turnos");
            }

            MercadoPagoConfig.AccessToken = _configuration["MercadoPago:AccessToken"];

            var preferenceRequest = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        Title = $"Turno en cancha {turno.Cancha} - {turno.Fecha.ToShortDateString()}",
                        Quantity = 1,
                        CurrencyId = "ARS",
                        UnitPrice = 16000m
                    }
                },
                ExternalReference = turnoId.ToString(),

                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = Url.Action("ConfirmarPago", "Pago", null, Request.Scheme),
                    Failure = Url.Action("ErrorPago", "Pago", null, Request.Scheme),
                    Pending = Url.Action("PendientePago", "Pago", null, Request.Scheme)
                },

                AutoReturn = "approved"
            };

            var client = new PreferenceClient();
            Preference preference = await client.CreateAsync(preferenceRequest);

            ViewBag.PreferenceId = preference.Id;
            ViewBag.PublicKey = _configuration["MercadoPago:PublicKey"];

            return View(); // Vista: CrearPago.cshtml
        }

        // ✅ Paso 2: Confirmar el pago y marcar turno como pagado
        public async Task<IActionResult> ConfirmarPago()
        {
            string paymentId = Request.Query["payment_id"];
            if (string.IsNullOrEmpty(paymentId))
            {
                TempData["Error"] = "No se pudo confirmar el pago.";
                return RedirectToAction("MisTurnos", "Turnos");
            }

            MercadoPagoConfig.AccessToken = _configuration["MercadoPago:AccessToken"];

            var client = new PaymentClient();
            var payment = await client.GetAsync(long.Parse(paymentId));

            if (payment.Status == "approved")
            {
                int turnoId = int.Parse(payment.ExternalReference);

                var turno = await _context.Turnos.FindAsync(turnoId);
                if (turno != null && !turno.EstaPagado)
                {
                    turno.EstaPagado = true;
                    _context.Turnos.Update(turno);
                    await _context.SaveChangesAsync();

                    TempData["Mensaje"] = "¡Pago exitoso! El turno ha sido marcado como pagado.";
                }
            }
            else
            {
                TempData["Error"] = "El pago no fue aprobado.";
            }

            return RedirectToAction("MisTurnos", "Turnos");
        }

        // ✅ Paso 3: Manejo de errores o estados pendientes
        public IActionResult ErrorPago()
        {
            TempData["Error"] = "El pago fue cancelado o falló.";
            return RedirectToAction("MisTurnos", "Turnos");
        }

        public IActionResult PendientePago()
        {
            TempData["Error"] = "El pago está pendiente.";
            return RedirectToAction("MisTurnos", "Turnos");
        }
    }
}
