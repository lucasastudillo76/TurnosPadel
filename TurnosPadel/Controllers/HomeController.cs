using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TurnosPadel.Models;

namespace TurnosPadel.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var disciplinas = new List<(string nombre, string imagen)>
    {
        ("Padel", "fotopadel.jpg"),
        ("Tenis", "fotocanchatenis.jpeg"),
        ("Fútbol", "canchafutbol.jpeg"),
        ("Patin", "fotopatin.jpg"),
        ("Bochas", "canchabochas.jpg"),
        ("Natacion", "piletanatacion.jpg"),
        ("Voley", "canchavoley.jpg")
    };

            // Pasamos la lista de disciplinas a la vista
            ViewData["Disciplinas"] = disciplinas;

            return View();
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
