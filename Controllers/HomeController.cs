using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TP6_Gorojod_Schwartz_Waserman.Models;

namespace TP6_Gorojod_Schwartz_Waserman.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
    [HttpPost]
    public IActionResult iniciarPartida(string username)
    {
        BD bd = new BD();
        // If user provides a username, check for an existing partida with that exact name
        if (!string.IsNullOrWhiteSpace(username))
        {
            var existing = bd.ObtenerPartidaPorNombre(username.Trim());
            if (existing != null)
            {
                HttpContext.Session.SetString("PartidaId", existing.Id.ToString());
                // Clear any leftover TempData messages when resuming a partida
                TempData.Remove("Sala1Mensaje");
                // resume at the saved room
                return RedirectToAction("Sala", new { IdSala = existing.IdSala });
            }
        }

        // otherwise create a new partida starting at Sala 1
        Partida partida = new Partida()
        { fechaInicio = DateTime.Today, horaInicio = DateTime.Now, IdSala = 1, estadoActual = "curso", nombreJugador = username };
        partida.Id = bd.crearPartida(partida);
        HttpContext.Session.SetString("PartidaId", partida.Id.ToString());
        // Clear any leftover TempData messages when starting a new partida
        TempData.Remove("Sala1Mensaje");
        return RedirectToAction("Sala", new { IdSala = 1 });
    }

    public IActionResult Historia()
    {   
        return View();
    }
    public IActionResult Index()
    {
        return View();
    }
    public IActionResult pasarSala (int IdSala) // es un método que sirve para pasar de una sala a otra cuando se completa un desafío. Se llama desde la vista de la sala actual y redirige a la vista de la siguiente sala.
    {
        BD bd = new BD();
        string? partidaIdSession = HttpContext.Session.GetString("PartidaId");

        if (string.IsNullOrEmpty(partidaIdSession))
            return RedirectToAction("Historia");

        int partidaId = int.Parse(partidaIdSession);
        int nIdSala = IdSala + 1;
        bd.pasarSala(partidaId, nIdSala);
        return RedirectToAction("Sala", new { IdSala = nIdSala });
    }

    public IActionResult irAForja()
    {
        string? partidaIdSession = HttpContext.Session.GetString("PartidaId");
        if (string.IsNullOrEmpty(partidaIdSession))
            return RedirectToAction("Historia");

        int partidaId = int.Parse(partidaIdSession);
        BD bd = new BD();
        // Actualizar la partida a Sala 3
        bd.pasarSala(partidaId, 3);
        
        HttpContext.Session.SetString("EnForja", "true");
        return RedirectToAction("Sala", new { IdSala = 3 });
    }

    public IActionResult Sala(int IdSala)
    {
        BD BD = new BD();
        string? partidaIdSession = HttpContext.Session.GetString("PartidaId");
        if (string.IsNullOrEmpty(partidaIdSession)) return RedirectToAction("Historia");

        int partidaId = int.Parse(partidaIdSession);
        bool puedeEntrar = BD.TieneAcceso(partidaId, IdSala);
        if (!puedeEntrar) return RedirectToAction("AccesoDenegado");

        string? enForja = HttpContext.Session.GetString("EnForja");
        string viewName;
        
        if (IdSala == 3)
        {
            viewName = "Sala3";
        }
        else
        {
            viewName = IdSala == 2 ? "Sala2" : "Sala" + IdSala;
        }
        
        return View(viewName);
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

    [HttpPost]
    public IActionResult ValidarSala1(string q1, string q2, string q3)
    {
        var respuestasCorrectas = new[]
        {
            "Renacimiento",
            "Imperio Romano",
            "Leonardo da Vinci"
        };

        var respuestas = new[] { q1, q2, q3 };
        int aciertos = 0;

        foreach (var respuesta in respuestas)
        {
            if (string.IsNullOrWhiteSpace(respuesta)) continue;
            if (respuestasCorrectas.Any(rc => string.Equals(rc, respuesta.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                aciertos++;
            }
        }

        if (aciertos == 3)
        {
            string? partidaIdSession = HttpContext.Session.GetString("PartidaId");
            if (!string.IsNullOrEmpty(partidaIdSession))
            {
                int partidaId = int.Parse(partidaIdSession);
                new BD().pasarSala(partidaId, 2);
            }

            TempData["Sala1Mensaje"] = "Has ganado la confianza de Oro, Sheo y Mato. El Aguijón roto te ha sido entregado.";
            return RedirectToAction("Sala", new { IdSala = 2 });
        }

        TempData["Sala1Mensaje"] = $"No lograste ganar la confianza completa. Respondiste bien {aciertos} de 3 preguntas. Intenta otra vez.";
        return RedirectToAction("Sala", new { IdSala = 1 });
    }
}
