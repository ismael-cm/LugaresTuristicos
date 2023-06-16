using LugaresTuristicos.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Xml.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace LugaresTuristicos.Controllers
{
    [Authorize]
    public class ValoracionController : Controller
    {
        SitesContext _context = new SitesContext();

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "USUARIO")]
        [HttpPost]
        public IActionResult Store(string comentario, string puntuacion, string IdLugar)
        {
            int IdUsuario = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            int? IdValoracion;

            var Existing = _context.LugaresValoraciones.Where(x => x.IdUsuario == IdUsuario && x.IdLugar == int.Parse(IdLugar)).FirstOrDefault();

            if (Existing == null)
            {
                //Create new
                LugaresValoracione ValoracionLugar = new LugaresValoracione();

                ValoracionLugar.IdLugar = int.Parse(IdLugar);
                ValoracionLugar.IdValoracion = int.Parse(puntuacion);
                ValoracionLugar.IdUsuario = IdUsuario;
                ValoracionLugar.Descripcion = string.IsNullOrEmpty(comentario) ? "" : comentario;
                ValoracionLugar.Fecha = DateTime.Now;

                _context.LugaresValoraciones.Add(ValoracionLugar);
                IdValoracion = ValoracionLugar.IdValoracion;
            } else
            {
                //Update existing
                Existing.IdValoracion = int.Parse(puntuacion);
                Existing.Descripcion = comentario;
                IdValoracion = Existing.IdValoracion;
            }



            _context.SaveChanges();

            

            return RedirectToAction("Show", new { IdValoracion = IdValoracion });
        }

        [AllowAnonymous]
        public JsonResult Show(string IdValoracion)
        {
           LugaresValoracione ValoracionLugar = _context.LugaresValoraciones
                .Where(l => l.IdValoracion == int.Parse(IdValoracion)).First();

            return Json(ValoracionLugar);
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult ShowValoracionesLugar(string IdLugar)
        {
            var ValoracionLugar = _context.LugaresValoraciones
                 .Where(l => l.IdLugar == int.Parse(IdLugar)).Average(l => l.IdValoracion);

            return Json(ValoracionLugar);
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult GetValoraciones(string IdLugar)
        {
            

            var VarloracionLugar = (from lugaresValoraciones in _context.LugaresValoraciones
                            join usuario in _context.Usuarios
                            on lugaresValoraciones.IdUsuario equals usuario.IdUsuario
                            where lugaresValoraciones.IdLugar == int.Parse(IdLugar)
                            orderby lugaresValoraciones.Fecha descending
                            select new
                            {
                                fecha = lugaresValoraciones.Fecha,
                                imagen = usuario.Imagen,
                                nombre = usuario.Nombre,
                                apellido = usuario.Apellido,
                                comentario = string.IsNullOrEmpty(lugaresValoraciones.Descripcion) ? "" : lugaresValoraciones.Descripcion,
                                idUsuario=usuario.IdUsuario,
                                idComentario= lugaresValoraciones.IdLValoracion,
                                puntuacion = lugaresValoraciones.IdValoracion
                            }).ToList();


            return Json(VarloracionLugar);
        }

        [HttpPost]
        public JsonResult ValoracionUsusarioLugar(string IdLugar)
        {
            int IdUsuario = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var ValoracionLugar = _context.LugaresValoraciones
                 .Where(l => l.IdLugar == int.Parse(IdLugar) && l.IdUsuario == IdUsuario).FirstOrDefault();

            return Json(ValoracionLugar);
        }
    }
}
