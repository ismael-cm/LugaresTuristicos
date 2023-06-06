using LugaresTuristicos.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Xml.Linq;
using System.Security.Claims;

namespace LugaresTuristicos.Controllers
{
    public class ValoracionController : Controller
    {
        SitesContext _context = new SitesContext();
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Store(string comentario, string puntuacion, string IdLugar)
        {
            int IdUsuario = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            LugaresValoracione ValoracionLugar = new LugaresValoracione();

            ValoracionLugar.IdLugar = int.Parse(IdLugar);
            ValoracionLugar.IdValoracion = int.Parse(puntuacion);
            ValoracionLugar.IdUsuario = IdUsuario;
            ValoracionLugar.Descripcion = puntuacion;
            ValoracionLugar.Fecha = DateTime.Now;

            _context.LugaresValoraciones.Add(ValoracionLugar);
            _context.SaveChanges();

            

            return RedirectToAction("Show", new { IdValoracion = ValoracionLugar.IdValoracion });
        }

        
        public JsonResult Show(string IdValoracion)
        {
           LugaresValoracione ValoracionLugar = _context.LugaresValoraciones
                .Where(l => l.IdValoracion == int.Parse(IdValoracion)).First();

            return Json(ValoracionLugar);
        }

        public JsonResult ShowValoracionesLugar(string IdLugar)
        {
            LugaresValoracione ValoracionLugar = _context.LugaresValoraciones
                 .Where(l => l.IdLugar == int.Parse(IdLugar)).First();

            return Json(ValoracionLugar);
        }
    }
}
