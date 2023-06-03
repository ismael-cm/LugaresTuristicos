using LugaresTuristicos.Commod;
using LugaresTuristicos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LugaresTuristicos.Controllers
{
    public class TuristaController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private SitesContext _dbContext = new SitesContext();
        private ValidationClass validationClass = new ValidationClass();

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            try
            {
                // Verificar si el usuario ha iniciado sesión
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
                {
                    // Si no ha iniciado sesión, redirigir al inicio de sesión
                    return RedirectToAction("Login"); // Reemplaza "Account" con el controlador y acción de inicio de sesión en tu aplicación
                }

                if (TempData["IsLoggedIn"] != null && (bool)TempData["IsLoggedIn"])
                    TempData["NameUser"] = TempData["IsLoggedInNameUser"];

                List<Lugare> lista = _dbContext.Lugares.Include(l => l.Comentarios)
                                                       .Include(l => l.IdMunicipioNavigation)
                                                       .Include(l => l.IdMunicipioNavigation.IdDeptoNavigation)
                                                       .Include(l => l.IdCategoriaNavigation)
                                                       .Include(l => l.IdUsuarioNavigation)
                                                       .ToList();

                var currentUserId = HttpContext.Session.GetString("IdUser");
                var currentUser = _dbContext.Usuarios.FirstOrDefault(s => s.IdUsuario.Equals(int.Parse(currentUserId)));
                CommonProfile allData = new CommonProfile();
                allData.Usuario = currentUser;
                allData.Lugares = lista;
                return View(allData);
            }
            catch (Exception ex)
            {
                Lugare model = new Lugare();
                return View(model);
            }
        }

        public IActionResult PostDetails(int? id)
        {
            
            try
            {
                TempData["d"] = id;
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
                {
                    // Si no ha iniciado sesión, redirigir al inicio de sesión
                    return RedirectToAction("Login", "Home"); // Reemplaza "Account" con el controlador y acción de inicio de sesión en tu aplicación
                }



                List<Lugare> currentPlace = _dbContext.Lugares.Include(l => l.Comentarios)
                                                       .Include(l => l.IdMunicipioNavigation)
                                                       .Include(l => l.IdMunicipioNavigation.IdDeptoNavigation)
                                                       .Include(l => l.IdCategoriaNavigation)
                                                       .Include(l => l.IdUsuarioNavigation)
                                                       .Where(x => x.IdLugar.Equals(id))
                                                       .ToList();

               
                return View(currentPlace);
            }
            catch(Exception e)
            {
                Lugare model = new Lugare();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateComment(string IdLugar, string Comentario)
        {
            try
            {
                Comentario comentario = new Comentario();
                comentario.IdLugar = Convert.ToInt32(IdLugar);
                comentario.Comentario1 = Comentario;
                comentario.Fecha = DateTime.Now;
                comentario.Estado = true;
                comentario.IdUsuario = Convert.ToInt32(HttpContext.Session.GetString("IdUser"));

                _dbContext.Comentarios.Add(comentario);
                _dbContext.SaveChanges();
                //g
            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("PostDetails", routeValues: new { id = Convert.ToInt32(IdLugar) });
        }

        [HttpPost]
        public JsonResult AllComentarios(int idLugar)
        {
            var comments = _dbContext.Comentarios.Where(x => x.IdLugar.Equals(idLugar)).First();
            return Json(comments);
        }
    }
}
