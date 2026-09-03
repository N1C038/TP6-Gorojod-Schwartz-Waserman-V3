using System.Diagnostics;
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
        Partida partida = new Partida()
            { fechaInicio = DateTime.Today, horaInicio = DateTime.Now, IdSala = 1, estadoActual = "curso", nombreJugador = username};
        BD bd = new BD();
        partida.Id = bd.crearPartida(partida);
        HttpContext.Session.SetString("PartidaId", partida.Id.ToString());
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
    public IActionResult Sala(int IdSala)
    {
        BD BD = new BD();
        string? partidaIdSession = HttpContext.Session.GetString("PartidaId");
        if (string.IsNullOrEmpty(partidaIdSession)) return RedirectToAction("Historia");

        int partidaId = int.Parse(partidaIdSession);
        bool puedeEntrar = BD.TieneAcceso(partidaId, IdSala);
        if (!puedeEntrar) return RedirectToAction("AccesoDenegado");

        var viewName = IdSala == 2 ? "Sala2Combat" : "Sala" + IdSala;
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
            if (respuestasCorrectas.Contains(respuesta))
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

            TempData["Sala1Mensaje"] = "Has ganado la confianza de Oro, Sheo y Mato. El Aguijón Puro te ha sido entregado.";
            return RedirectToAction("Sala", new { IdSala = 2 });
        }

        TempData["Sala1Mensaje"] = $"No lograste ganar la confianza completa. Respondiste bien {aciertos} de 3 preguntas. Intenta otra vez.";
        return RedirectToAction("Sala", new { IdSala = 1 });
    }
}
